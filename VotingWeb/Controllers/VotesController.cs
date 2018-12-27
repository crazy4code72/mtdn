namespace VotingWeb.Controllers
{
    using global::VotingWeb.Model;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Fabric.Query;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using global::VotingWeb.Helper;

    [Produces("application/json")]
    [Route("api/[controller]")]
    public class VotesController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly string reverseProxyBaseUri;
        private readonly StatelessServiceContext serviceContext;
        private readonly IHttpContextAccessor httpContextAccessor;

        /// <summary>
        /// Constructor
        /// </summary>
        public VotesController(HttpClient httpClient, StatelessServiceContext context, FabricClient fabricClient, IHttpContextAccessor httpContextAccessor)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            serviceContext = context;
            this.httpContextAccessor = httpContextAccessor;
            reverseProxyBaseUri = Environment.GetEnvironmentVariable("ReverseProxyBaseUri");
        }

        // GET: api/Votes
        //        [HttpGet("")]
        //        public async Task<IActionResult> Get()
        //        {
        //            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
        //            Uri proxyAddress = GetProxyAddress(serviceName);
        //
        //            ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);
        //
        //            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();
        //
        //            foreach (Partition partition in partitions)
        //            {
        //                string proxyUrl =
        //                    $"{proxyAddress}/api/VoteData?PartitionKey={((Int64RangePartitionInformation) partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";
        //
        //                using (HttpResponseMessage response = await httpClient.GetAsync(proxyUrl))
        //                {
        //                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
        //                    {
        //                        continue;
        //                    }
        //
        //                    result.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(await response.Content.ReadAsStringAsync()));
        //                }
        //            }
        //
        //            return Json(result);
        //        }

        // PUT: api/Votes/name
//        [HttpPut("{name}")]
//        public async Task<IActionResult> Put(string name)
//        {
//            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
//            Uri proxyAddress = GetProxyAddress(serviceName);
//            long partitionKey = GetPartitionKey(name);
//            string proxyUrl = $"{proxyAddress}/api/VoteData/{name}?PartitionKey={partitionKey}&PartitionKind=Int64Range";
//
//            StringContent putContent = new StringContent($"{{ 'name' : '{name}' }}", Encoding.UTF8, "application/json");
//            putContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
//
//            using (HttpResponseMessage response = await httpClient.PutAsync(proxyUrl, putContent))
//            {
//                return new ContentResult
//                {
//                    StatusCode = (int) response.StatusCode,
//                    Content = await response.Content.ReadAsStringAsync()
//                };
//            }
//        }

        // DELETE: api/Votes/name
//        [HttpDelete("{name}")]
//        public async Task<IActionResult> Delete(string name)
//        {
//            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
//            Uri proxyAddress = GetProxyAddress(serviceName);
//            long partitionKey = GetPartitionKey(name);
//            string proxyUrl = $"{proxyAddress}/api/VoteData/{name}?PartitionKey={partitionKey}&PartitionKind=Int64Range";
//
//            using (HttpResponseMessage response = await httpClient.DeleteAsync(proxyUrl))
//            {
//                if (response.StatusCode != System.Net.HttpStatusCode.OK)
//                {
//                    return StatusCode((int) response.StatusCode);
//                }
//            }
//
//            return new OkResult();
//        }

        /// <summary>
        /// Constructs a reverse proxy URL for a given service.
        /// Example: http://localhost:19081/VotingApplication/VotingData/
        /// </summary>
        /// <param name="serviceName"></param>
        /// <returns></returns>
        private Uri GetProxyAddress(Uri serviceName)
        {
            return new Uri($"{reverseProxyBaseUri}{serviceName.AbsolutePath}");
        }

        /// <summary>
        /// Get live voting results.
        /// </summary>
        /// <returns></returns>
        // GET: api/Votes/LiveVotingResult
        [Throttle(ThrottleOn = ThrottleOn.IpAddress, AllowedRequestCount = 1, TimeDurationInSeconds = 7)]
        [Throttle(ThrottleOn = ThrottleOn.Path, AllowedRequestCount = 50, TimeDurationInSeconds = 1)]
        [HttpGet("LiveVotingResult")]
        public async Task<IActionResult> GetLiveVotingResult()
        {
            try
            {
                Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
                Uri proxyAddress = GetProxyAddress(serviceName);

                ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);
                List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();

                foreach (Partition partition in partitions)
                {
                    string proxyUrl =
                        $"{proxyAddress}/api/VoteData/LiveVotingResult?PartitionKey={((Int64RangePartitionInformation)partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                    using (HttpResponseMessage response = await httpClient.GetAsync(proxyUrl))
                    {
                        if (response.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            continue;
                        }

                        result.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(await response.Content.ReadAsStringAsync()));
                    }
                }

                return Json(result);
            }
            catch (Exception)
            {
                return Json(new List<KeyValuePair<string, string>>());
            }
        }

        /// <summary>
        /// Submit Aadhar no to send Otp
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/SubmitAadharNoToSendOtp/aadharNo
        [Throttle(ThrottleOn = ThrottleOn.IpAddress, TimeDurationInSeconds = 900, AllowedRequestCount = 5)]
        [Throttle(ThrottleOn = ThrottleOn.Path, TimeDurationInSeconds = 1, AllowedRequestCount = 100)]
        [HttpPost("SubmitAadharNoToSendOtp/{aadharNo}")]
        public async Task<IActionResult> SubmitAadharNoToSendOtp(string aadharNo)
        {
            try
            {
                IActionResult validationResult = Validator.ValidateAadharNoToSendOtp(aadharNo);
                if (validationResult != null) return validationResult;

                Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
                Uri proxyAddress = GetProxyAddress(serviceName);
                int partitionKey = aadharNo.Sum(c => (int)char.GetNumericValue(c));
                string proxyUrl = $"{proxyAddress}/api/VoteData/SubmitAadharNoToSendOtp/{aadharNo}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

                StringContent postContent = new StringContent($"{{ 'aadharNo' : '{aadharNo}' }}", Encoding.UTF8, "application/json");
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (HttpResponseMessage response = await httpClient.PostAsync(proxyUrl, postContent))
                {
                    return ActionResultCreator.CreateActionResult(response);
                }
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.ResponseMessageCode.Failure.ToString()
                };
            }
        }

        /// <summary>
        /// Verify Otp for Aadhar no.
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <param name="userEnteredOtp">User entered otp</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/VerifyOtp/aadharNo/userEnteredOtp
        [Throttle(ThrottleOn = ThrottleOn.Path, AllowedRequestCount = 10, TimeDurationInSeconds = 1)]
        [Throttle(ThrottleOn = ThrottleOn.IpAddress, AllowedRequestCount = 10, TimeDurationInSeconds = 900)]
        [HttpPost("VerifyOtp/{aadharNo}/{userEnteredOtp}")]
        public async Task<IActionResult> VerifyOtp(string aadharNo, string userEnteredOtp)
        {
            try
            {
                IActionResult validationResult = Validator.ValidateVerifyOtpData(aadharNo, userEnteredOtp);
                if (validationResult != null) return validationResult;

                Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
                Uri proxyAddress = GetProxyAddress(serviceName);
                int partitionKey = aadharNo.Sum(c => (int)char.GetNumericValue(c));
                string proxyUrl = $"{proxyAddress}/api/VoteData/VerifyOtp/{aadharNo}/{userEnteredOtp}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

                StringContent postContent = new StringContent($"{{ 'aadharNo' : '{aadharNo}', 'userEnteredOtp' : '{userEnteredOtp}' }}", Encoding.UTF8, "application/json");
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                using (HttpResponseMessage response = await httpClient.PostAsync(proxyUrl, postContent))
                {
                    return ActionResultCreator.CreateActionResult(response);
                }
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.ResponseMessageCode.Failure.ToString()
                };
            }
        }

        /// <summary>
        /// Link Voter Id to Aadhar.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/LinkVoterIdToAadhar
        [Throttle(ThrottleOn = ThrottleOn.Path, AllowedRequestCount = 10, TimeDurationInSeconds = 1)]
        [Throttle(ThrottleOn = ThrottleOn.IpAddress, AllowedRequestCount = 10, TimeDurationInSeconds = 900)]
        [HttpPost("LinkVoterIdToAadhar")]
        public async Task<IActionResult> LinkVoterIdToAadhar([FromBody] UserDetails userDetails)
        {
            try
            {
                IActionResult validationResult = Validator.ValidateLinkVoterIdToAadharData(userDetails);
                if (validationResult != null) return validationResult;

                Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
                Uri proxyAddress = GetProxyAddress(serviceName);
                int partitionKey = userDetails.AadharNo.Sum(c => (int)char.GetNumericValue(c));
                string proxyUrl = $"{proxyAddress}/api/VoteData/LinkVoterIdToAadhar?PartitionKey={partitionKey}&PartitionKind=Int64Range";

                StringContent postContent = new StringContent($"{{ 'AadharNo' : '{userDetails.AadharNo}', 'VoterId' : '{userDetails.VoterId}', 'Name' : '{userDetails.Name}', 'DOB' : '{userDetails.DOB}', 'FatherName' : '{userDetails.FatherName}', 'Gender' : '{userDetails.Gender}', 'Otp' : '{userDetails.Otp}' }}", Encoding.UTF8, "application/json");
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (HttpResponseMessage response = await httpClient.PostAsync(proxyUrl, postContent))
                {
                    return ActionResultCreator.CreateVoterIdLinkActionResult(response);
                }
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.VoterIdLinkingStatus.LinkingFailed.ToString()
                };
            }
        }

        /// <summary>
        /// Cast vote.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/CastVote
        [Throttle(ThrottleOn = ThrottleOn.IpAddress, TimeDurationInSeconds = 900, AllowedRequestCount = 5)]
        [Throttle(ThrottleOn = ThrottleOn.Path, TimeDurationInSeconds = 1, AllowedRequestCount = 100)]
        [HttpPost("CastVote")]
        public async Task<IActionResult> CastVote([FromBody] UserDetails userDetails)
        {
            try
            {
                IActionResult validationResult = Validator.ValidateCastVoteData(userDetails);
                if (validationResult != null) return validationResult;

                Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
                Uri proxyAddress = GetProxyAddress(serviceName);
                int partitionKey = userDetails.AadharNo.Sum(c => (int)char.GetNumericValue(c));
                string proxyUrl = $"{proxyAddress}/api/VoteData/CastVote?PartitionKey={partitionKey}&PartitionKind=Int64Range";

                StringContent postContent = new StringContent($"{{ 'AadharNo' : '{userDetails.AadharNo}', 'VoterId' : '{userDetails.VoterId}', 'VoteFor' : '{userDetails.VoteFor}', 'Otp' : '{userDetails.Otp}' }}", Encoding.UTF8, "application/json");
                postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                using (HttpResponseMessage response = await httpClient.PostAsync(proxyUrl, postContent))
                {
                    return ActionResultCreator.CreateCastVoteActionResult(response);
                }
            }
            catch (Exception)
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.CastVoteStatus.VotingFailed.ToString()
                };
            }
        }
    }
}