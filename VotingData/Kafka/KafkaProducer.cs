using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VotingData.Kafka
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using Confluent.Kafka.Serialization;

    using LogMessage = Confluent.Kafka.LogMessage;

    /// <summary>
    /// The kafka producer.
    /// </summary>
    /// <typeparam name="T1">
    /// key dataType
    /// </typeparam>
    /// <typeparam name="T2">
    /// value dataType
    /// </typeparam>
    public class KafkaProducer<T1, T2> : IDisposable
    {
        /// <summary>
        /// Server address
        /// </summary>
        private const string ServerAddressConstant = "bootstrap.servers";

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
        /// The debug logging level.
        /// </summary>
        private const string DebugLoggingLevel = "debug";

        /// <summary>
        /// The compression codec.
        /// </summary>
        private const string CompressionCodec = "compression.codec";

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly KafkaProducerProperties configuration;

        /// <summary>
        /// The producer.
        /// </summary>
        private readonly Producer<T1, T2> producer;

        /// <summary>
        /// The topic name.
        /// </summary>
        private readonly string topicName;

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaProducer{T1,T2}"/> class.
        /// </summary>
        /// <param name="topicName">
        /// The topic name.
        /// </param>
        /// <param name="configValueDictionary">
        /// The config value dictionary.
        /// </param>
        /// <param name="kafkaProducerValidator">
        /// The kafka Producer Validator.
        /// </param>
        /// <param name="keySerializer">
        /// The key serializer.
        /// </param>
        /// <param name="valueSerializer">
        /// The value serializer.
        /// </param>
        public KafkaProducer(
            string topicName,
            Dictionary<string, object> configValueDictionary,
            ISerializer<T1> keySerializer,
            ISerializer<T2> valueSerializer)
        {
            this.topicName = topicName;
            this.producer = new Producer<T1, T2>(configValueDictionary, keySerializer, valueSerializer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KafkaProducer{T1,T2}"/> class.
        /// </summary>
        /// <param name="configuration">
        /// The configuration.
        /// </param>
        /// <param name="kafkaProducerValidator">
        /// The kafka Producer Validator.
        /// </param>
        /// <param name="keySerializer">
        /// The key serializer.
        /// </param>
        /// <param name="valueSerializer">
        /// The value serializer.
        /// </param>
        public KafkaProducer(
            KafkaProducerProperties configuration,
            ISerializer<T1> keySerializer,
            ISerializer<T2> valueSerializer)
        {
            this.topicName = configuration.TopicName;
            this.configuration = configuration;

            var config = new Dictionary<string, object>
                             {
                                 {
                                     ServerAddressConstant, string.Join(",", this.configuration.ServerAddresses)
                                 }
                             };

            if (this.configuration.UseSecureKafka)
            {
                config.Add(SecurityProtocol, this.configuration.SecurityProtocol);
                config.Add(SslCertificateLocation, this.configuration.CertificateLocation);
                config.Add(SslKeyLocation, this.configuration.KeyLocation);
                config.Add(SslKeyPassword, this.configuration.KeyPassword);
                config.Add(SslCaLocation, this.configuration.CertificateAuthorityLocation);
            }

            config.Add("socket.blocking.max.ms", "1"); //min =1 and this is a windows specific fix documented here https://github.com/confluentinc/confluent-kafka-dotnet/issues/501
            config.Add(CompressionCodec, this.configuration.CompressionType.ToString());

            if (!string.IsNullOrWhiteSpace(this.configuration.LoggingLevel))
            {
                config.Add(DebugLoggingLevel, this.configuration.LoggingLevel);
            }

            this.producer = new Producer<T1, T2>(config, keySerializer, valueSerializer);

            if (config.ContainsKey(DebugLoggingLevel) && !string.IsNullOrWhiteSpace((string)config[DebugLoggingLevel]))
            {
                this.producer.OnLog += this.OnLog;
            }
        }

        /// <inheritdoc />
        public void Dispose()
        {
            this.producer.Dispose();
        }

        /// <inheritdoc />
        public async Task ProduceAsync(T1 key, T2 obj)
        {
            try
            {
                await this.producer.ProduceAsync(this.topicName, key, obj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error occurred while sending message to kafka topic {this.topicName}", ex);
            }
        }

        /// <summary>
        /// The event to log kafka client activities
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        /// <exception cref="NotImplementedException">
        /// Not Implemented exception
        /// </exception>
        private void OnLog(object sender, LogMessage e)
        {
            Console.WriteLine("ConfluentKafka : [{0}] [{1}] [{2}] [{3}]", e.Name, e.Facility, e.Level, e.Message);
        }

        /// <summary>
        /// Validates kafka producer configurations
        /// </summary>
        private void Validate()
        {
            if (this.configuration == null)
            {
                throw new ArgumentNullException($"{nameof(this.configuration)}", "No Kafka Producer Configuration is found");
            }

            if (this.configuration.ServerAddresses.Any(string.IsNullOrWhiteSpace))
            {
                throw new Exception("Missing bootstrap server address configuration.");
            }
        }
    }
}
