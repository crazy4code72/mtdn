namespace Database
{
    using System;
    using System.Fabric;
    using System.Threading;
    using Autofac;
    using Autofac.Integration.ServiceFabric;
    using Kafka;

    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                var builder = new ContainerBuilder();
                // Register any regular dependencies.
                var databaseParams = DatabaseConsumerParameters.GetDatabaseConsumerParameters(FabricRuntime.GetActivationContext());
                builder.Register(c => databaseParams).As<DatabaseConsumerParameters>().SingleInstance();
                builder.Register(c => GetKafkaProducerProperties(c.Resolve<DatabaseConsumerParameters>()))
                    .As<KafkaProducerProperties>().SingleInstance();
                // Register the Autofac for Service Fabric support.
                builder.RegisterServiceFabricSupport();

                // Register a stateless service...
                builder.RegisterStatelessService<DatabaseConsumer>("VotingWeb");

                using (builder.Build())
                {
//                    ServiceEventSource.Current.ServiceTypeRegistered(
//                        Process.GetCurrentProcess().Id,
//                        typeof(DatabaseConsumer).Name);

                    // Prevents this host process from terminating so services keep running.
                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                //ServiceEventSource.Current.ServiceHostInitializationFailed(e.ToString());
                throw;
            }
        }

        private static KafkaProducerProperties GetKafkaProducerProperties(
            DatabaseConsumerParameters databaseConsumerParameters)
        {
            var kafkaProducerProperties = new KafkaProducerProperties
            {
                ServerAddresses = databaseConsumerParameters.KafkaProducerServerAddress,
                TopicName = databaseConsumerParameters.KafkaProducerTopicName,
                UseSecureKafka = databaseConsumerParameters.UseSecureKafka,
                SecurityProtocol = databaseConsumerParameters.SecurityProtocol,
                CertificateLocation = databaseConsumerParameters.CertificateLocation,
                KeyLocation = databaseConsumerParameters.KeyLocation,
                KeyPassword = databaseConsumerParameters.KeyPassword,
                CertificateAuthorityLocation = databaseConsumerParameters.CertificateAuthorityLocation
            };

            return kafkaProducerProperties;
        }
    }
}
