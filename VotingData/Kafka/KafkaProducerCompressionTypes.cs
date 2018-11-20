namespace VotingData.Kafka
{
    public enum KafkaProducerCompressionTypes
    {
        /// <summary>
        /// The none.
        /// </summary>
        None = 0,

        /// <summary>
        /// The gzip.
        /// </summary>
        Gzip = 1,

        /// <summary>
        /// The snappy.
        /// </summary>
        Snappy = 2,

        /// <summary>
        /// The lz 4.
        /// </summary>
        Lz4 = 3
    }
}
