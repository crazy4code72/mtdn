namespace VotingData.Kafka
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// The KafkaProducer interface.
    /// </summary>
    /// <typeparam name="T1">
    /// key type object
    /// </typeparam>
    /// <typeparam name="T2">
    /// value type object
    /// </typeparam>
    public interface IKafkaProducer<in T1, in T2> : IDisposable
    {
        /// <summary>
        /// Produces the message to kafka queue.
        /// </summary>
        /// <param name="key">
        /// the partition key
        /// </param>
        /// <param name="obj">
        ///  actual object to be queued
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task ProduceAsync(T1 key, T2 obj);
    }
}
