using System.Net.Http;

namespace VotingData.Controllers
{
    using Confluent.Kafka.Serialization;
    using global::VotingData.Handlers;
    using global::VotingData.Kafka;
    using global::VotingData.Model;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using VotingDatabase;

    [Route("api/[controller]")]
    public class VoteDataController : Controller
    {
        /// <summary>
        /// State manager.
        /// </summary>
        private readonly IReliableStateManager stateManager;

        /// <summary>
        /// Voting database parameters.
        /// </summary>
        private readonly VotingDatabaseParameters votingDatabaseParameters;

        /// <summary>
        /// Otp verification handler.
        /// </summary>
        private IOtpVerificationHandler otpVerificationHandler;

        /// <summary>
        /// Voter id link handler.
        /// </summary>
        private IVoterIdLinkHandler voterIdLinkHandler;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stateManager">State manager</param>
        /// <param name="votingDatabaseParameters">Database parameters</param>
        /// <param name="otpVerificationHandler">Otp verification handler</param>
        public VoteDataController(IReliableStateManager stateManager/*,
                                  VotingDatabaseParameters votingDatabaseParameters,
                                  IOtpVerificationHandler otpVerificationHandler*/)
        {
            this.stateManager = stateManager;
            //this.votingDatabaseParameters = votingDatabaseParameters;
            //this.otpVerificationHandler = otpVerificationHandler;
        }

        // GET api/VoteData
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            CancellationToken ct = new CancellationToken();

            IReliableDictionary<string, int> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                IAsyncEnumerable<KeyValuePair<string, int>> list = await votesDictionary.CreateEnumerableAsync(tx);

                IAsyncEnumerator<KeyValuePair<string, int>> enumerator = list.GetAsyncEnumerator();

                List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();

                while (await enumerator.MoveNextAsync(ct))
                {
                    result.Add(enumerator.Current);
                }

                return Json(result);
            }
        }

        // PUT api/VoteData/name
        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name)
        {
            IReliableDictionary<string, int> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await votesDictionary.AddOrUpdateAsync(tx, name, 1, (key, oldValue) => oldValue + 1);
                await tx.CommitAsync();
            }

            return new OkResult();
        }

        // DELETE api/VoteData/name
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            IReliableDictionary<string, int> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                if (await votesDictionary.ContainsKeyAsync(tx, name))
                {
                    await votesDictionary.TryRemoveAsync(tx, name);
                    await tx.CommitAsync();
                    return new OkResult();
                }

                return new NotFoundResult();
            }
        }

        /// <summary>
        /// Submit Aadhar No to send Otp
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <returns>Action result</returns>
        // POST api/VoteData/SubmitAadharNoToSendOtp/aadharNo
        [HttpPost("SubmitAadharNoToSendOtp/{aadharNo}")]
        public async Task<IActionResult> SubmitAadharNoToSendOtp(string aadharNo)
        {
            var userDetails = new UserDetails
            {
                AadharNo = aadharNo,
                EventType = Enums.EventType.SendOtp
            };
            var kafkaTopicProduceStatus = await ProduceToKafkaTopic(userDetails);
            return new ContentResult { StatusCode = (int)kafkaTopicProduceStatus };
        }

        /// <summary>
        /// Verify Otp for the Aadhar no.
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <param name="userEnteredOtp">User entered otp</param>
        /// <returns>Action result</returns>
        // POST api/VoteData/VerifyOtp/aadharNo/userEnteredOtp
        [HttpPost("VerifyOtp/{aadharNo}/{userEnteredOtp}")]
        public async Task<IActionResult> VerifyOtp(string aadharNo, string userEnteredOtp)
        {
            if (this.otpVerificationHandler == null)
            {
                this.otpVerificationHandler = new OtpVerificationHandler();
            }
            var otpVerifiedFromDb = this.otpVerificationHandler.VerifyOtp(aadharNo, userEnteredOtp);
            bool otpUpdatedInState = false;
            if (otpVerifiedFromDb)
            {
                otpUpdatedInState = await UpsertKeyValuePairInState(aadharNo, userEnteredOtp, Enums.StateName.AadharNoOtpPair.ToString());
            }

            var statusCode = otpVerifiedFromDb && otpUpdatedInState ? (int)Enums.ResponseMessageCode.Success : (int)Enums.ResponseMessageCode.Failure;
            return new ContentResult { StatusCode = statusCode };
        }

        /// <summary>
        /// Link Voter Id to Aadhar.
        /// Check if this voter id is already linked, check in state.
        /// If Yes, return Already Linked (No db call)
        /// If No, make db call, try to Link.
        /// Add in state only after Successfully Linked.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/LinkVoterIdToAadhar
        [HttpPost("LinkVoterIdToAadhar")]
        public async Task<IActionResult> LinkVoterIdToAadhar([FromBody] UserDetails userDetails)
        {
            var voterIdLinkedFoundInState = await CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.VoterId, Enums.StateName.AadharNoVoterIdPair.ToString());
            if (voterIdLinkedFoundInState)
            {
                return new ContentResult { StatusCode = (int)Enums.VoterIdLinkingStatus.AlreadyLinked };
            }

            // Check state to verify if Otp is correct.
            var otpCorrectInState = await CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.Otp.ToString(), Enums.StateName.AadharNoOtpPair.ToString());
            if (otpCorrectInState)
            {
                if (this.voterIdLinkHandler == null)
                {
                    this.voterIdLinkHandler = new VoterIdLinkHandler();
                }
                var voterIdLinkingStatus = this.voterIdLinkHandler.LinkVoterIdToAadhar(userDetails);
                int? statusCode;
                switch (voterIdLinkingStatus)
                {
                    case Enums.VoterIdLinkingStatus.Unauthorized: statusCode = (int)Enums.VoterIdLinkingStatus.Unauthorized; break;
                    case Enums.VoterIdLinkingStatus.LinkingFailed: statusCode = (int)Enums.VoterIdLinkingStatus.LinkingFailed; break;
                    case Enums.VoterIdLinkingStatus.AlreadyLinked: statusCode = (int)Enums.VoterIdLinkingStatus.AlreadyLinked; break;
                    case Enums.VoterIdLinkingStatus.SuccessfullyLinked: statusCode = (int)Enums.VoterIdLinkingStatus.SuccessfullyLinked; break;
                    default: statusCode = (int)Enums.VoterIdLinkingStatus.LinkingFailed; break;
                }

                if (statusCode.Equals((int)Enums.VoterIdLinkingStatus.SuccessfullyLinked))
                {
                    await UpsertKeyValuePairInState(userDetails.AadharNo, userDetails.VoterId, Enums.StateName.AadharNoVoterIdPair.ToString());
                }

                return new ContentResult { StatusCode = statusCode };
            }

            return new ContentResult { StatusCode = (int)Enums.VoterIdLinkingStatus.Unauthorized };
        }

        /// <summary>
        /// Cast vote.
        /// Check if this voter id has a voted candidate in state.
        /// If Yes, return Already Voted (No producing to kafka topic).
        /// If No, produce to kafka topic.
        /// Add in state only after producing to kafka topic.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/CastVote
        [HttpPost("CastVote")]
        public async Task<IActionResult> CastVote([FromBody] UserDetails userDetails)
        {
            userDetails.EventType = Enums.EventType.CastVote;

            // Check in state of VotingResult.
            var voterIdHasVotedCandidateInState = await CheckIfKeyValuePairExistInState(userDetails.VoterId, bool.TrueString, Enums.StateName.VoterIdVoteForPair.ToString());
            if (voterIdHasVotedCandidateInState)
            {
                return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.AlreadyVoted };
            }

            // Check state of Voting and Otp to verify Aadhar No, Voter Id and Otp.
            var isAadharAndVoterIdValid = await CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.VoterId, Enums.StateName.AadharNoVoterIdPair.ToString());
            var isAadharAndOtpValid = await CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.Otp.ToString(), Enums.StateName.AadharNoOtpPair.ToString());
            if (!isAadharAndVoterIdValid || !isAadharAndOtpValid)
            {
                return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.VotingFailed };
            }

            // Produce to kafka topic
            var kafkaTopicProduceStatus = await ProduceToKafkaTopic(userDetails);
            if (kafkaTopicProduceStatus.Equals(Enums.ResponseMessageCode.Success))
            {
                bool votingResultUpdated = await UpsertKeyValuePairInState(userDetails.VoterId, bool.TrueString, Enums.StateName.VoterIdVoteForPair.ToString());
                if (votingResultUpdated)
                {
                    return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.SuccessfullyVoted };
                }
            }

            // Return voting failed.
            return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.VotingFailed };
        }

        /// <summary>
        /// Check if key value pair exist in state.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="name">Name</param>
        /// <returns>Bool</returns>
        private async Task<bool> CheckIfKeyValuePairExistInState(string key, string value, string name)
        {
            CancellationToken ct = new CancellationToken();
            IReliableDictionary<string, string> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(name);

            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                IAsyncEnumerable<KeyValuePair<string, string>> list = await votesDictionary.CreateEnumerableAsync(tx);
                IAsyncEnumerator<KeyValuePair<string, string>> enumerator = list.GetAsyncEnumerator();

                KeyValuePair<string, string> keyValuePair;
                while (await enumerator.MoveNextAsync(ct))
                {
                    keyValuePair = enumerator.Current;
                    if (keyValuePair.Key.Equals(key) && keyValuePair.Value.Equals(value))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Update key value pair in state.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="name">Name</param>
        /// <returns>bool</returns>
        private async Task<bool> UpsertKeyValuePairInState(string key, string value, string name)
        {
            try
            {
                IReliableDictionary<string, string> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(name);

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    await votesDictionary.AddOrUpdateAsync(tx, key, value, (oldKey, oldValue) => value);
                    await tx.CommitAsync();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Produce to kafka topic.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        private async Task<Enums.ResponseMessageCode> ProduceToKafkaTopic(UserDetails userDetails)
        {
            //TODO: Read these values from config.
            var config = new KafkaProducerProperties
            {
                ServerAddresses = new List<string> { "127.0.0.1:9092" },
                TopicName = "Voting",
                CompressionType = KafkaProducerCompressionTypes.Snappy
            };

            using (KafkaProducer<string, string> producer = new KafkaProducer<string, string>(config, new StringSerializer(Encoding.UTF8), new StringSerializer(Encoding.UTF8)))
            {
                List<Task> tasks = new List<Task>();
                try
                {
                    tasks.Add(producer.ProduceAsync(userDetails.AadharNo, JsonConvert.SerializeObject(userDetails)));

                    if (tasks.Count == 100)
                    {
                        await Task.WhenAll(tasks);
                    }
                }
                catch (Exception)
                {
                    return Enums.ResponseMessageCode.Failure;
                }

                await Task.WhenAll(tasks);
            }

            return Enums.ResponseMessageCode.Success;
        }
    }
}