using System;
using System.Diagnostics;
using System.Text;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using VotingData.Kafka;
using VotingData.Model;

namespace VotingData.Controllers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;

    [Route("api/[controller]")]
    public class VoteDataController : Controller
    {
        private readonly IReliableStateManager stateManager;

        public VoteDataController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
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

        // POST api/VoteData/SubmitAadharNoToSendOtp/aadharNo
        [HttpPost("SubmitAadharNoToSendOtp/{aadharNo}")]
        public async Task<IActionResult> SubmitAadharNoToSendOtp(string aadharNo)
        {
            // Add code to produce it to kafka topic
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
                    // Publish user details to Kafka
                    var userDetails = new UserDetails
                    {
                        AadharNo = aadharNo,
                        EventType = "SendOtp"
                    };
                    tasks.Add(producer.ProduceAsync(aadharNo, JsonConvert.SerializeObject(userDetails)));

                    if (tasks.Count == 100)
                    {
                        await Task.WhenAll(tasks);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                await Task.WhenAll(tasks);
            }

            return new OkResult();
        }
    }
}