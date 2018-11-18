namespace Kafka
{
    using System.Collections.Generic;
    using System;
    using Confluent.Kafka;
    using Confluent.Kafka.Serialization;

    /// <summary>
    /// The kafka consumer.
    /// </summary>
    /// <typeparam name="T1">key</typeparam>
    /// <typeparam name="T2">value</typeparam>
    public class KafkaConsumer<T1, T2>
    {
        /// <summary>
        /// The consumer group id.
        /// </summary>
        private const string ConsumerGroupId = "group.id";

        /// <summary>
        /// The server addresses.
        /// </summary>
        private const string ServerAddresses = "bootstrap.servers";

        /// <summary>
        /// The auto commit interval in milliseconds.
        /// </summary>
        private const string AutoCommitIntervalInMilliseconds = "auto.commit.interval.ms";

        /// <summary>
        /// The auto offset reset.
        /// </summary>
        private const string AutoOffsetReset = "auto.offset.reset";

        /// <summary>
        /// The maximum partition fetch bytes.
        /// </summary>
        private const string MaximumPartitionFetchBytes = "max.partition.fetch.bytes";

        /// <summary>
        /// The logging level.
        /// </summary>
        private const string LoggingLevel = "debug";

        /// <summary>
        /// The security protocol.
        /// </summary>
        private const string SecurityProtocol = "security.protocol";

        /// <summary>
        /// The ssl certificate location.
        /// </summary>
        private const string SslCertificateLocation = "ssl.certificate.location";

        /// <summary>
        /// The ssl key location.
        /// </summary>
        private const string SslKeyLocation = "ssl.key.location";

        /// <summary>
        /// The ssl key password.
        /// </summary>
        private const string SslKeyPassword = "ssl.key.password";

        /// <summary>
        /// The ssl ca location.
        /// </summary>
        private const string SslCaLocation = "ssl.ca.location";

        /// <summary>
        /// The consumer.
        /// </summary>
        private readonly Consumer<T1, T2> consumer;

        /// <summary>
        /// The kafka consumer properties.
        /// </summary>
        private readonly KafkaConsumerProperties kafkaConsumerProperties;

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConsumer{T1,T2}"/> class.
        /// </summary>
        public KafkaConsumer(
            string topicName,
            Dictionary<string, object> configuration,
            IDeserializer<T1> deserializer1,
            IDeserializer<T2> deserializer2)
        {
            this.consumer = new Consumer<T1, T2>(configuration, deserializer1, deserializer2);
            this.consumer.Subscribe(topicName);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaConsumer{T1,T2}"/> class.
        /// </summary>
        public KafkaConsumer(
            KafkaConsumerProperties consumerProperties,
            IDeserializer<T1> deserializer1,
            IDeserializer<T2> deserializer2)
        {
            this.kafkaConsumerProperties = consumerProperties;
            var configuration = this.GetKafkaConfiguration(consumerProperties);

            this.consumer = new Consumer<T1, T2>(configuration, deserializer1, deserializer2);
            this.consumer.Subscribe(this.kafkaConsumerProperties.TopicName);
        }

        /// <summary>
        /// The consume.
        /// </summary>
        /// <param name="pollInterval">
        /// The poll interval.
        /// </param>
        /// <returns>
        /// The <see cref="Message"/>.
        /// </returns>
        public Message<T1, T2> Consume(TimeSpan pollInterval)
        {
            Message<T1, T2> message;

            try
            {
                if (this.consumer.Consume(out message, (int)pollInterval.TotalMilliseconds))
                {
                    return message;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while consuming message from kafka topic {this.kafkaConsumerProperties.TopicName}", ex);
            }

            return message;
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.consumer?.Dispose();
        }

        /// <summary>
        /// The get kafka configuration.
        /// </summary>
        private Dictionary<string, object> GetKafkaConfiguration(KafkaConsumerProperties consumerProperties)
        {
            string serverAddresses = string.Join(",", consumerProperties.ServerAddresses);

            var configuration = new Dictionary<string, object>()
            {
                [ConsumerGroupId] = consumerProperties.ConsumerGroupId,
                [ServerAddresses] = serverAddresses
            };

            if (consumerProperties.AutoCommitIntervalInMilliseconds != null)
            {
                configuration.Add(AutoCommitIntervalInMilliseconds, consumerProperties.AutoCommitIntervalInMilliseconds);
            }

            if (consumerProperties.AutoOffsetReset != null)
            {
                configuration.Add(AutoOffsetReset, consumerProperties.AutoOffsetReset);
            }

            if (consumerProperties.MaximumParititionFetchBytes != null)
            {
                configuration.Add(MaximumPartitionFetchBytes, consumerProperties.MaximumParititionFetchBytes);
            }

            if (!string.IsNullOrWhiteSpace(consumerProperties.LoggingLevel))
            {
                configuration.Add(LoggingLevel, consumerProperties.LoggingLevel);
            }

            if (consumerProperties.UseSecureKafka)
            {
                configuration.Add(SecurityProtocol, consumerProperties.SecurityProtocol);
                configuration.Add(SslCertificateLocation, consumerProperties.CertificateLocation);
                configuration.Add(SslKeyLocation, consumerProperties.KeyLocation);
                configuration.Add(SslKeyPassword, consumerProperties.KeyPassword);
                configuration.Add(SslCaLocation, consumerProperties.CertificateAuthorityLocation);
            }

            return configuration;
        }
    }
}
