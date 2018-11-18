namespace Kafka
{
    using System.Collections.Generic;

    /// <summary>
    /// The kafka consumer properties.
    /// </summary>
    public class KafkaConsumerProperties
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConsumerProperties"/> class.
        /// </summary>
        public KafkaConsumerProperties()
        {
            this.ServerAddresses = new List<string>();
        }

        /// <summary>
        /// Gets or sets the topic name.
        /// </summary>
        public string TopicName { get; set; }

        /// <summary>
        /// Gets or sets the consumer group id.
        /// </summary>
        public string ConsumerGroupId { get; set; }

        /// <summary>
        /// Gets or sets the server addresses.
        /// </summary>
        public List<string> ServerAddresses { get; set; }

        /// <summary>
        /// Gets or sets the auto commit interval in milliseconds.
        /// </summary>
        public int? AutoCommitIntervalInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the auto offset reset.
        /// </summary>
        public string AutoOffsetReset { get; set; }

        /// <summary>
        /// Gets or sets the maximum paritition fetch bytes.
        /// </summary>
        public long? MaximumParititionFetchBytes { get; set; }

        /// <summary>
        /// Gets or sets the debug logging level.
        /// supported values:
        /// generic, broker, topic, metadata, feature, queue, msg, protocol, cgrp, security, fetch, interceptor, plugin, consumer, admin, all
        /// </summary>
        public string LoggingLevel { get; set; }

        /// <summary>
        /// Gets or sets the certificate location.
        /// </summary>
        public string CertificateLocation { get; set; }

        /// <summary>
        /// Gets or sets the key location.
        /// </summary>
        public string KeyLocation { get; set; }

        /// <summary>
        /// Gets or sets the security protocol.
        /// </summary>
        public string SecurityProtocol { get; set; }

        /// <summary>
        /// Gets or sets the key password.
        /// </summary>
        public string KeyPassword { get; set; }

        /// <summary>
        /// Gets or sets the certificate authority location.
        /// </summary>
        public string CertificateAuthorityLocation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use secure kafka.
        /// </summary>
        public bool UseSecureKafka { get; set; }
    }
}
