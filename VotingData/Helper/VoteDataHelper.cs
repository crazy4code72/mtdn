namespace VotingData.Helper
{
    using Confluent.Kafka.Serialization;
    using global::VotingData.Kafka;
    using global::VotingData.Model;
    using Microsoft.ServiceFabric.Data;
    using Microsoft.ServiceFabric.Data.Collections;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public class VoteDataHelper
    {
        /// <summary>
        /// State manager.
        /// </summary>
        private readonly IReliableStateManager stateManager;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stateManager"></param>
        public VoteDataHelper(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }


        /// <summary>
        /// Check if key value pair exist in state.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <param name="name">Name</param>
        /// <returns>Bool</returns>
        internal async Task<bool> CheckIfKeyValuePairExistInState(string key, string value, string name)
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
        internal async Task<bool> UpsertKeyValuePairInState(string key, string value, string name)
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
        internal async Task<Enums.ResponseMessageCode> ProduceToKafkaTopic(UserDetails userDetails)
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
