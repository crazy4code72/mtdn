using System;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace VotingDatabase.Kafka
{
    /// <summary>
    /// The KafkaConsumer interface.
    /// </summary>
    /// <typeparam name="T1">key
    /// </typeparam>
    /// <typeparam name="T2">value
    /// </typeparam>
    public interface IKafkaConsumer<T1, T2>
    {
        /// <summary>
        /// The consume.
        /// </summary>
        /// <param name="pollInterval">
        /// The poll interval.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        Message<T1, T2> Consume(TimeSpan pollInterval);

        /// <summary>
        /// The dispose.
        /// </summary>
        void Dispose();

        /// <summary>
        /// The commit async.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task CommitAsync();
    }
}
