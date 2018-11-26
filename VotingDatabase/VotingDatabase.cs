namespace VotingDatabase
{
    using System;
    using global::VotingDatabase.Handlers;
    using VotingData.Kafka;
    using VotingData.Model;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Newtonsoft.Json;

    public class VotingDatabase : StatelessService
    {
        /// <summary>
        /// The database consumer parameters.
        /// </summary>
        private readonly VotingDatabaseParameters votingDatabaseParameters;

        /// <summary>
        /// The kafka consumer.
        /// </summary>
        private readonly IKafkaConsumer<string, string> kafkaConsumer;

        /// <summary>
        /// Voting database message handler.
        /// </summary>
        private readonly IVotingDatabaseMessageHandler votingDatabaseMessageHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="VotingDatabase"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="votingDatabaseMessageHandler">Voting database message handler.</param>
        /// <param name="votingDatabaseParameters">The database consumer parameters.</param>
        /// <param name="kafkaConsumer">The kafka consumer.</param>
        public VotingDatabase(
                StatelessServiceContext context,
                IVotingDatabaseMessageHandler votingDatabaseMessageHandler,
                VotingDatabaseParameters votingDatabaseParameters,
                IKafkaConsumer<string, string> kafkaConsumer)
            : base(context)
        {
            this.votingDatabaseParameters = votingDatabaseParameters;
            this.kafkaConsumer = kafkaConsumer;
            this.votingDatabaseMessageHandler = votingDatabaseMessageHandler;
        }

        /// <summary>
        /// Optional override to create listeners (e.g., TCP, HTTP) for this service replica to handle client or user requests.
        /// </summary>
        /// <returns>
        /// A collection of listeners.
        /// </returns>
        protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
        {
            return new ServiceInstanceListener[0];
        }

        /// <summary>
        /// This is the main entry point for our service instance.
        /// </summary>
        /// <param name="cancellationToken">Canceled when Service Fabric needs to shut down this service instance.</param>
        /// <returns>The <see cref="Task"/>.</returns>
        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    this.kafkaConsumer?.Dispose();
                }

                cancellationToken.ThrowIfCancellationRequested();

                // TODO: Put a throttle here on number of requests.
                var message = this.kafkaConsumer?.Consume(this.votingDatabaseParameters.MessagePollIntervalInMilliseconds);

                if (message == null) continue;
                try
                {
                    var userDetails = JsonConvert.DeserializeObject<UserDetails>(message.Value);
                    await this.votingDatabaseMessageHandler.HandleMessage(userDetails);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }
    }
}