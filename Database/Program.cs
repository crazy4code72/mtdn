namespace VotingDatabase
{
    using System;
    using System.Fabric;
    using System.Threading;
    using Autofac;
    using Autofac.Integration.ServiceFabric;
    using Kafka;
    using System.Diagnostics;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Database;

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

                // Register the Autofac for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register a stateless service.
                builder.RegisterStatelessService<VotingDatabase>("VotingDatabase");

                using (builder.Build())
                {
                    ServiceRuntime.RegisterServiceAsync("VotingDatabaseType", context => new VotingDatabase(context)).GetAwaiter().GetResult();
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
    }
}