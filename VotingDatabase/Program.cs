namespace VotingDatabase
{
    using System.Text;
    using Confluent.Kafka.Serialization;
    using VotingData.Kafka;
    using System;
    using System.Fabric;
    using System.Threading;
    using Autofac;
    using Autofac.Integration.ServiceFabric;
    using System.Diagnostics;
    using global::VotingDatabase.Handlers;
    using VotingData.Model;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = new ContainerBuilder();

                // Register any regular dependencies.
                var databaseParams = VotingDatabaseParameters.GetDatabaseConsumerParameters(FabricRuntime.GetActivationContext());
                builder.Register(c => databaseParams).As<VotingDatabaseParameters>().SingleInstance();
                builder.RegisterType<KafkaConsumer<string, string>>().As<IKafkaConsumer<string, string>>().SingleInstance();
                builder.RegisterType<VotingDatabaseMessageHandler>().As<IVotingDatabaseMessageHandler>().SingleInstance();
                builder.RegisterType<OtpHandler>().As<IDataHandler>().Keyed<IDataHandler>(Enums.EventType.SendOtp);
                builder.RegisterType<OtpHandler>().As<IDataHandler>().Keyed<IDataHandler>(Enums.EventType.VerifyOtp);
                builder.Register(c => GetKafkaConsumerProperties(c.Resolve<VotingDatabaseParameters>())).As<KafkaConsumerProperties>().SingleInstance();
                builder.Register(c => SetupKafkaConsumer(c.Resolve<VotingDatabaseParameters>())).As<IKafkaConsumer<string, string>>().SingleInstance();

                builder.Register<Func<Enums.EventType, IDataHandler>>(c =>
                {
                    var componentContext = c.Resolve<IComponentContext>();
                    return eventType => componentContext.ResolveKeyed<IDataHandler>(eventType);
                });

                // Register the Autofac for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register a stateless service.
                builder.RegisterStatelessService<VotingDatabase>("VotingDatabaseType");

                using (builder.Build())
                {
                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(VotingDatabase).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        /// <summary>
        /// The setup kafka consumer.
        /// </summary>
        /// <param name="databaseConsumerParameters">
        /// The database consumer parameters.
        /// </param>
        /// <returns>
        /// The <see cref="KafkaConsumer{T1,T2}"/>.
        /// </returns>
        private static IKafkaConsumer<string, string> SetupKafkaConsumer(VotingDatabaseParameters databaseConsumerParameters)
        {
            var kafkaConsumerProperties = GetKafkaConsumerProperties(databaseConsumerParameters);

            return new KafkaConsumer<string, string>(
                kafkaConsumerProperties,
                new StringDeserializer(Encoding.UTF8),
                new StringDeserializer(Encoding.UTF8));
        }

        private static KafkaConsumerProperties GetKafkaConsumerProperties(VotingDatabaseParameters databaseConsumerParameters)
        {
            var kafkaConsumerProperties = new KafkaConsumerProperties
            {
                AutoCommitIntervalInMilliseconds = databaseConsumerParameters.KafkaAutoCommitIntervalInMilliseconds,
                AutoOffsetReset = databaseConsumerParameters.KafkaAutoOffsetReset,
                ServerAddresses = databaseConsumerParameters.KafkaListenerServerAddress,
                TopicName = databaseConsumerParameters.KafkaListenerTopicName,
                ConsumerGroupId = databaseConsumerParameters.KafkaConsumerGroupId,
                MaximumParititionFetchBytes = databaseConsumerParameters.KafkaMaxPartitionFetchBytes,
                UseSecureKafka = databaseConsumerParameters.UseSecureKafka,
                SecurityProtocol = databaseConsumerParameters.SecurityProtocol,
                CertificateLocation = databaseConsumerParameters.CertificateLocation,
                KeyLocation = databaseConsumerParameters.KeyLocation,
                KeyPassword = databaseConsumerParameters.KeyPassword,
                CertificateAuthorityLocation = databaseConsumerParameters.CertificateAuthorityLocation,
            };
            return kafkaConsumerProperties;
        }
    }
}