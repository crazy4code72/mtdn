namespace VotingWeb.Controllers
{
    using global::VotingWeb.Model;
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

    [Produces("application/json")]
    [Route("api/[controller]")]
    public class VotesController : Controller
    {
        private readonly HttpClient httpClient;
        private readonly FabricClient fabricClient;
        private readonly string reverseProxyBaseUri;
        private readonly StatelessServiceContext serviceContext;

        /// <summary>
        /// Constructor
        /// </summary>
        public VotesController(HttpClient httpClient, StatelessServiceContext context, FabricClient fabricClient)
        {
            this.fabricClient = fabricClient;
            this.httpClient = httpClient;
            serviceContext = context;
            reverseProxyBaseUri = Environment.GetEnvironmentVariable("ReverseProxyBaseUri");
        }

        // GET: api/Votes
        [HttpGet("")]
        public async Task<IActionResult> Get()
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
            Uri proxyAddress = GetProxyAddress(serviceName);

            ServicePartitionList partitions = await fabricClient.QueryManager.GetPartitionListAsync(serviceName);

            List<KeyValuePair<string, int>> result = new List<KeyValuePair<string, int>>();

            foreach (Partition partition in partitions)
            {
                string proxyUrl =
                    $"{proxyAddress}/api/VoteData?PartitionKey={((Int64RangePartitionInformation) partition.PartitionInformation).LowKey}&PartitionKind=Int64Range";

                using (HttpResponseMessage response = await httpClient.GetAsync(proxyUrl))
                {
                    if (response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        continue;
                    }

                    result.AddRange(JsonConvert.DeserializeObject<List<KeyValuePair<string, int>>>(await response.Content.ReadAsStringAsync()));
                }
            }

            return Json(result);
        }

        // PUT: api/Votes/name
        [HttpPut("{name}")]
        public async Task<IActionResult> Put(string name)
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
            Uri proxyAddress = GetProxyAddress(serviceName);
            long partitionKey = GetPartitionKey(name);
            string proxyUrl = $"{proxyAddress}/api/VoteData/{name}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

            StringContent putContent = new StringContent($"{{ 'name' : '{name}' }}", Encoding.UTF8, "application/json");
            putContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpResponseMessage response = await httpClient.PutAsync(proxyUrl, putContent))
            {
                return new ContentResult
                {
                    StatusCode = (int) response.StatusCode,
                    Content = await response.Content.ReadAsStringAsync()
                };
            }
        }

        // DELETE: api/Votes/name
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
            Uri proxyAddress = GetProxyAddress(serviceName);
            long partitionKey = GetPartitionKey(name);
            string proxyUrl = $"{proxyAddress}/api/VoteData/{name}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

            using (HttpResponseMessage response = await httpClient.DeleteAsync(proxyUrl))
            {
                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return StatusCode((int) response.StatusCode);
                }
            }

            return new OkResult();
        }

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
        /// Creates a partition key from the given name.
        /// Uses the zero-based numeric position in the alphabet of the first letter of the name (0-25).
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private long GetPartitionKey(string name)
        {
            return Char.ToUpper(name.First()) - 'A';
        }

        /// <summary>
        /// Submit Aadhar no to send Otp
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/SubmitAadharNoToSendOtp/aadharNo
        [HttpPost("SubmitAadharNoToSendOtp/{aadharNo}")]
        public async Task<IActionResult> SubmitAadharNoToSendOtp(string aadharNo)
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
            Uri proxyAddress = GetProxyAddress(serviceName);
            int partitionKey = aadharNo.Sum(c => (int) char.GetNumericValue(c));
            string proxyUrl = $"{proxyAddress}/api/VoteData/SubmitAadharNoToSendOtp/{aadharNo}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

            StringContent postContent = new StringContent($"{{ 'aadharNo' : '{aadharNo}' }}", Encoding.UTF8, "application/json");
            postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpResponseMessage response = await httpClient.PostAsync(proxyUrl, postContent))
            {
                return CreateActionResult(response);
            }
        }

        /// <summary>
        /// Verify Otp for Aadhar no.
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <param name="userEnteredOtp">User entered otp</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/VerifyOtp/aadharNo/userEnteredOtp
        [HttpPost("VerifyOtp/{aadharNo}/{userEnteredOtp}")]
        public async Task<IActionResult> VerifyOtp(string aadharNo, string userEnteredOtp)
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
            Uri proxyAddress = GetProxyAddress(serviceName);
            int partitionKey = aadharNo.Sum(c => (int)char.GetNumericValue(c));
            string proxyUrl = $"{proxyAddress}/api/VoteData/VerifyOtp/{aadharNo}/{userEnteredOtp}?PartitionKey={partitionKey}&PartitionKind=Int64Range";

            StringContent postContent = new StringContent($"{{ 'aadharNo' : '{aadharNo}' }}, {{ 'userEnteredOtp' : '{userEnteredOtp}' }}", Encoding.UTF8, "application/json");
            postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using (HttpResponseMessage response = await httpClient.PostAsync(proxyUrl, postContent))
            {
                return CreateActionResult(response);
            }
        }

        /// <summary>
        /// Link Voter Id to Aadhar.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        // POST: api/Votes/LinkVoterIdToAadhar
        [HttpPost("LinkVoterIdToAadhar")]
        public async Task<IActionResult> LinkVoterIdToAadhar([FromBody] UserDetails userDetails)
        {
            Uri serviceName = VotingWeb.GetVotingDataServiceName(serviceContext);
            Uri proxyAddress = GetProxyAddress(serviceName);
            int partitionKey = userDetails.AadharNo.Sum(c => (int)char.GetNumericValue(c));
            string proxyUrl = $"{proxyAddress}/api/VoteData/LinkVoterIdToAadhar?PartitionKey={partitionKey}&PartitionKind=Int64Range";

            StringContent postContent = new StringContent($"{{ 'AadharNo' : '{userDetails.AadharNo}', 'VoterId' : '{userDetails.VoterId}', 'Name' : '{userDetails.Name}', 'DOB' : '{userDetails.DOB}', 'FatherName' : '{userDetails.FatherName}', 'Gender' : '{userDetails.Gender}' }}", Encoding.UTF8, "application/json");
            postContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            using (HttpResponseMessage response = await httpClient.PostAsync(proxyUrl, postContent))
            {
                return CreateVoterIdLinkActionResult(response);
            }
        }

        /// <summary>
        /// Create action result
        /// </summary>
        /// <param name="response">Http response</param>
        /// <returns>Action result</returns>
        private static IActionResult CreateActionResult(HttpResponseMessage response)
        {
            return new ContentResult
            {
                StatusCode = (int)Enums.ResponseMessageCode.Success,
                Content = (int)response.StatusCode == (int)Enums.ResponseMessageCode.Success
                            ? Enums.ResponseMessageCode.Success.ToString()
                            : Enums.ResponseMessageCode.Failure.ToString()
            };
        }

        /// <summary>
        /// Create action result
        /// </summary>
        /// <param name="response">Http response</param>
        /// <returns>Action result</returns>
        private static IActionResult CreateVoterIdLinkActionResult(HttpResponseMessage response)
        {
            string voterIdLinkStatus = string.Empty;
            switch ((int)response.StatusCode)
            {
                case (int)Enums.VoterIdLinkingStatus.Unauthorized : voterIdLinkStatus = Enums.VoterIdLinkingStatus.Unauthorized.ToString(); break;
                case (int)Enums.VoterIdLinkingStatus.LinkingFailed: voterIdLinkStatus = Enums.VoterIdLinkingStatus.LinkingFailed.ToString(); break;
                case (int)Enums.VoterIdLinkingStatus.AlreadyLinked: voterIdLinkStatus = Enums.VoterIdLinkingStatus.AlreadyLinked.ToString(); break;
                case (int)Enums.VoterIdLinkingStatus.SuccessfullyLinked: voterIdLinkStatus = Enums.VoterIdLinkingStatus.SuccessfullyLinked.ToString(); break;
            }

            return new ContentResult
            {
                StatusCode = (int)Enums.ResponseMessageCode.Success,
                Content = voterIdLinkStatus
            };
        }
    }
}