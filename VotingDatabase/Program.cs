using System.Text;
using Confluent.Kafka.Serialization;
using VotingData.Kafka;

namespace VotingDatabase
{
    using System;
    using System.Fabric;
    using System.Threading;
    using Autofac;
    using Autofac.Integration.ServiceFabric;
    using System.Diagnostics;
    using Microsoft.ServiceFabric.Services.Runtime;

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
                builder.Register(c => GetKafkaProducerProperties(c.Resolve<VotingDatabaseParameters>())).As<KafkaProducerProperties>().SingleInstance();
                builder.Register(c => SetupKafkaProducer(c.Resolve<VotingDatabaseParameters>())).As<KafkaProducer<string, string>>().SingleInstance();

                // Register the Autofac for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register a stateless service.
                builder.RegisterStatelessService<VotingDatabase>("VotingDatabaseType");

                using (builder.Build())
                {
                    //ServiceRuntime.RegisterServiceAsync("VotingDatabaseType", context => new VotingDatabase(context)).GetAwaiter().GetResult();
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

        private static KafkaProducerProperties GetKafkaProducerProperties(VotingDatabaseParameters votingDatabaseParameters)
        {
            return new KafkaProducerProperties
            {
                ServerAddresses = votingDatabaseParameters.KafkaProducerServerAddress,
                TopicName = votingDatabaseParameters.KafkaProducerTopicName,
                UseSecureKafka = votingDatabaseParameters.UseSecureKafka,
                SecurityProtocol = votingDatabaseParameters.SecurityProtocol,
                CertificateLocation = votingDatabaseParameters.CertificateLocation,
                KeyLocation = votingDatabaseParameters.KeyLocation,
                KeyPassword = votingDatabaseParameters.KeyPassword,
                CertificateAuthorityLocation = votingDatabaseParameters.CertificateAuthorityLocation,
                CompressionType = KafkaProducerCompressionTypes.Snappy
            };
        }

        /// <summary>
        /// The setup kafka producer.
        /// </summary>
        /// <param name="databaseConsumerParameters">The database consumer parameters.</param>
        /// <returns>The <see cref="KafkaProducer{T1,T2}"/>.</returns>
        private static KafkaProducer<string, string> SetupKafkaProducer(VotingDatabaseParameters databaseConsumerParameters)
        {
            var kafkaProducerProperties = GetKafkaProducerProperties(databaseConsumerParameters);

            return new KafkaProducer<string, string>(
                kafkaProducerProperties,
                new StringSerializer(Encoding.UTF8),
                new StringSerializer(Encoding.UTF8));
        }
    }
}