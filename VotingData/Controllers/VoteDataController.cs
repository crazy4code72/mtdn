namespace VotingData.Controllers
{
    using global::VotingData.Handlers;
    using global::VotingData.Helper;
    using global::VotingData.Model;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
        /// Vote data helper.
        /// </summary>
        private readonly VoteDataHelper voteDataHelper;

        /// <summary>
        /// Otp verification handler.
        /// </summary>
        private static readonly IOtpVerificationHandler otpVerificationHandler = new OtpVerificationHandler();

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
            this.voteDataHelper = new VoteDataHelper(stateManager);
            //this.votingDatabaseParameters = votingDatabaseParameters;
            //this.otpVerificationHandler = otpVerificationHandler;
        }

        // GET api/VoteData
        //        [HttpGet]
        //        public async Task<IActionResult> Get()
        //        {
        //            CancellationToken ct = new CancellationToken();
        //
        //            IReliableDictionary<string, int> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");
        //
        //            using (ITransaction tx = this.stateManager.CreateTransaction())
        //            {
        //                IAsyncEnumerable<KeyValuePair<string, int>> list = await votesDictionary.CreateEnumerableAsync(tx);
        //
        //                IAsyncEnumerator<KeyValuePair<string, int>> enumerator = list.GetAsyncEnumerator();
        //
        //                List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
        //
        //                while (await enumerator.MoveNextAsync(ct))
        //                {
        //                    result.Add(enumerator.Current);
        //                }
        //
        //                return Json(result);
        //            }
        //        }

        // PUT api/VoteData/name
        //        [HttpPut("{name}")]
        //        public async Task<IActionResult> Put(string name)
        //        {
        //            IReliableDictionary<string, int> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");
        //
        //            using (ITransaction tx = this.stateManager.CreateTransaction())
        //            {
        //                await votesDictionary.AddOrUpdateAsync(tx, name, 1, (key, oldValue) => oldValue + 1);
        //                await tx.CommitAsync();
        //            }
        //
        //            return new OkResult();
        //        }
        //
        //        // DELETE api/VoteData/name
        //        [HttpDelete("{name}")]
        //        public async Task<IActionResult> Delete(string name)
        //        {
        //            IReliableDictionary<string, int> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, int>>("counts");
        //
        //            using (ITransaction tx = this.stateManager.CreateTransaction())
        //            {
        //                if (await votesDictionary.ContainsKeyAsync(tx, name))
        //                {
        //                    await votesDictionary.TryRemoveAsync(tx, name);
        //                    await tx.CommitAsync();
        //                    return new OkResult();
        //                }
        //
        //                return new NotFoundResult();
        //            }
        //        }

        // GET api/VoteData
        [HttpGet("LiveVotingResult")]
        public async Task<IActionResult> Get()
        {
            try
            {
                CancellationToken ct = new CancellationToken();
                IReliableDictionary<string, string> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(Enums.StateName.VoterIdVoteForPair.ToString());

                using (ITransaction tx = this.stateManager.CreateTransaction())
                {
                    IAsyncEnumerable<KeyValuePair<string, string>> list = await votesDictionary.CreateEnumerableAsync(tx);
                    IAsyncEnumerator<KeyValuePair<string, string>> enumerator = list.GetAsyncEnumerator();
                    Dictionary<string, int> dictionary = new Dictionary<string, int>();

                    while (await enumerator.MoveNextAsync(ct))
                    {
                        if (dictionary.ContainsKey(enumerator.Current.Value))
                        {
                            dictionary[enumerator.Current.Value] = dictionary[enumerator.Current.Value] + 1;
                        }
                        else
                        {
                            dictionary[enumerator.Current.Value] = 1;
                        }
                    }

                    var result = dictionary.OrderByDescending(d => d.Value);
                    return Json(result);
                }
            }
            catch (Exception)
            {
                return Json(new List<KeyValuePair<string, string>>());
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
            try
            {
                var userDetails = new UserDetails
                {
                    AadharNo = aadharNo,
                    EventType = Enums.EventType.SendOtp
                };
                await voteDataHelper.ProduceToKafkaTopic(userDetails);
                return new ContentResult { StatusCode = (int)Enums.ResponseMessageCode.Success };
            }
            catch (Exception)
            {
                return new ContentResult { StatusCode = (int)Enums.ResponseMessageCode.Failure };
            }
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
            try
            {
                var otpVerifiedFromDb = otpVerificationHandler.VerifyOtp(aadharNo, userEnteredOtp);
                if (otpVerifiedFromDb)
                {
                    await voteDataHelper.UpsertKeyValuePairInState(aadharNo, userEnteredOtp, Enums.StateName.AadharNoOtpPair.ToString());
                }

                var statusCode = otpVerifiedFromDb ? (int)Enums.ResponseMessageCode.Success : (int)Enums.ResponseMessageCode.Failure;
                return new ContentResult { StatusCode = statusCode };
            }
            catch (Exception)
            {
                return new ContentResult { StatusCode = (int)Enums.ResponseMessageCode.Failure };
            }
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
            try
            {
                var voterIdLinkedFoundInState = await voteDataHelper.CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.VoterId, Enums.StateName.AadharNoVoterIdPair.ToString());
                if (voterIdLinkedFoundInState)
                {
                    return new ContentResult { StatusCode = (int)Enums.VoterIdLinkingStatus.AlreadyLinked };
                }

                // Check state to verify if Otp is correct.
                var otpCorrectInState = await voteDataHelper.CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.Otp.ToString(), Enums.StateName.AadharNoOtpPair.ToString());
                if (otpCorrectInState)
                {
                    if (this.voterIdLinkHandler == null)
                    {
                        this.voterIdLinkHandler = new VoterIdLinkHandler();
                    }
                    var voterIdLinkingStatus = voterIdLinkHandler.LinkVoterIdToAadhar(userDetails);
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
                        await voteDataHelper.UpsertKeyValuePairInState(userDetails.AadharNo, userDetails.VoterId, Enums.StateName.AadharNoVoterIdPair.ToString());
                    }

                    return new ContentResult { StatusCode = statusCode };
                }

                return new ContentResult { StatusCode = (int)Enums.VoterIdLinkingStatus.Unauthorized };
            }
            catch (Exception)
            {
                return new ContentResult { StatusCode = (int)Enums.VoterIdLinkingStatus.LinkingFailed };
            }
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
            try
            {
                userDetails.EventType = Enums.EventType.CastVote;

                // Check in state of VotingResult.
                var voterIdHasVotedCandidateInState = await voteDataHelper.CheckIfKeyValuePairExistInState(userDetails.VoterId, userDetails.VoteFor, Enums.StateName.VoterIdVoteForPair.ToString(), true);
                if (voterIdHasVotedCandidateInState)
                {
                    return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.AlreadyVoted };
                }

                // Check state of Voting and Otp to verify Aadhar No, Voter Id and Otp.
                var isAadharAndVoterIdValid = await voteDataHelper.CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.VoterId, Enums.StateName.AadharNoVoterIdPair.ToString());
                var isAadharAndOtpValid = await voteDataHelper.CheckIfKeyValuePairExistInState(userDetails.AadharNo, userDetails.Otp.ToString(), Enums.StateName.AadharNoOtpPair.ToString());
                if (!isAadharAndVoterIdValid || !isAadharAndOtpValid)
                {
                    return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.VotingFailed };
                }

                // Add to state and produce to kafka topic after succesful addition in state.
                await voteDataHelper.UpsertKeyValuePairInState(userDetails.VoterId, userDetails.VoteFor, Enums.StateName.VoterIdVoteForPair.ToString());

                try
                {
                    await voteDataHelper.ProduceToKafkaTopic(userDetails);
                }
                catch (Exception)
                {
                    await voteDataHelper.DeleteFromState(Enums.StateName.VoterIdVoteForPair.ToString(), userDetails.VoterId);
                    return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.VotingFailed };
                }

                return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.SuccessfullyVoted };
            }
            catch (Exception)
            {
                return new ContentResult { StatusCode = (int)Enums.CastVoteStatus.VotingFailed };
            }
        }
    }
}