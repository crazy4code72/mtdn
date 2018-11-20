using VotingData.Kafka;

namespace VotingDatabase
{
    using System.Collections.Generic;
    using System.Fabric;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;

    public class VotingDatabase  : StatelessService
    {
        /// <summary>
        /// The database consumer parameters.
        /// </summary>
        private readonly VotingDatabaseParameters _votingDatabaseParameters;

        /// <summary>
        /// The kafka consumer.
        /// </summary>
        private readonly KafkaConsumer<string, string> kafkaConsumer;

        public VotingDatabase(StatelessServiceContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VotingDatabase"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="votingDatabaseParameters">The database consumer parameters.</param>
        /// <param name="kafkaConsumer">The kafka consumer.</param>
        public VotingDatabase(
                StatelessServiceContext context,
                VotingDatabaseParameters votingDatabaseParameters,
                KafkaConsumer<string, string> kafkaConsumer)
            : base(context)
        {
            this._votingDatabaseParameters = votingDatabaseParameters;
            this.kafkaConsumer = kafkaConsumer;
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

            }
        }
    }
}