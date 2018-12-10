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
        /// <param name="ignoreValueCheck">Ignore checking of value, check only if key exists or not</param>
        /// <returns>Bool</returns>
        internal async Task<bool> CheckIfKeyValuePairExistInState(string key, string value, string name, bool ignoreValueCheck = false)
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
                    if (keyValuePair.Key.Equals(key))
                    {
                        return ignoreValueCheck || keyValuePair.Value.Equals(value);
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
        internal async Task UpsertKeyValuePairInState(string key, string value, string name)
        {
            IReliableDictionary<string, string> votesDictionary = await this.stateManager.GetOrAddAsync<IReliableDictionary<string, string>>(name);
            using (ITransaction tx = this.stateManager.CreateTransaction())
            {
                await votesDictionary.AddOrUpdateAsync(tx, key, value, (oldKey, oldValue) => value);
                await tx.CommitAsync();
            }
        }

        /// <summary>
        /// Produce to kafka topic.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        internal async Task ProduceToKafkaTopic(UserDetails userDetails)
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
                List<Task> tasks = new List<Task>
                {
                    producer.ProduceAsync(userDetails.AadharNo, JsonConvert.SerializeObject(userDetails))
                };

                if (tasks.Count == 100)
                {
                    await Task.WhenAll(tasks);
                }
                await Task.WhenAll(tasks);
            }
        }
    }
}
