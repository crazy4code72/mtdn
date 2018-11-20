using System.Collections.Generic;

namespace VotingData.Kafka
{
    /// <summary>
    /// The kafka producer config.
    /// </summary>
    public class KafkaProducerProperties
    {
        /// <summary>
        /// Gets or sets the server address.
        /// e.g. http://10.46.34.134:8090, http://10.34.24.123:6070
        /// </summary>
        public List<string> ServerAddresses { get; set; }

        /// <summary>
        /// Gets or sets the topic name.
        /// </summary>
        public string TopicName { get; set; }

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
        /// Gets or sets the debug logging level.
        /// supported values:
        /// generic, broker, topic, metadata, feature, queue, msg, protocol, cgrp, security, fetch, interceptor, plugin, consumer, admin, all
        /// </summary>
        public string LoggingLevel { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether use secure kafka.
        /// </summary>
        public bool UseSecureKafka { get; set; }

        /// <summary>
        /// Gets or sets the compression type.
        /// </summary>
        public KafkaProducerCompressionTypes CompressionType { get; set; }
    }
}
