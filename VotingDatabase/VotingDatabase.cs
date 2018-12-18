namespace VotingDatabase
{
    using global::VotingDatabase.Handlers;
    using global::VotingDatabase.Kafka;
    using global::VotingDatabase.Model;
    using Microsoft.ServiceFabric.Services.Communication.Runtime;
    using Microsoft.ServiceFabric.Services.Runtime;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Fabric;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Voting database.
    /// </summary>
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
        public VotingDatabase(StatelessServiceContext context,
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
            var userDetailsDictionary = CreateUserDetailsDictionary();
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    this.kafkaConsumer?.Dispose();
                }

                cancellationToken.ThrowIfCancellationRequested();

                var userDetailsTasks = userDetailsDictionary.Select(async userDetails =>
                {
                    var userDetailsBag = userDetailsDictionary[userDetails.Key];
                    var elapsedTimeInSeconds = (DateTime.UtcNow - userDetailsBag.BatchStartTime).TotalSeconds;
                    await ProcessUserDetailsBasedOnTiming(userDetailsBag, elapsedTimeInSeconds, this.votingDatabaseParameters.WaitTimeinSeconds.Seconds, cancellationToken);
                }).ToList();

                await Task.WhenAll(userDetailsTasks);

                var message = this.kafkaConsumer?.Consume(this.votingDatabaseParameters.MessagePollIntervalInMilliseconds);

                if (message == null) continue;
                try
                {
                    var userDetails = JsonConvert.DeserializeObject<UserDetails>(message.Value);

                    // Match the newly arrived user details event type and start processing it in batches.
                    foreach (var keyValuePair in userDetailsDictionary)
                    {
                        if (userDetails.EventType.ToString().Equals(keyValuePair.Key.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            await this.ProcessMsg(userDetailsDictionary[keyValuePair.Key], userDetails, cancellationToken);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore
                }
            }
        }

        /// <summary>
        /// Create user details dictionary.
        /// </summary>
        /// <returns>User details dictionary.</returns>
        private Dictionary<Enums.EventType, UserDetailsBag> CreateUserDetailsDictionary()
        {
            return new Dictionary<Enums.EventType, UserDetailsBag>
            {
                {
                    Enums.EventType.SendOtp, new UserDetailsBag(new List<UserDetails>(), DateTime.UtcNow)
                },
                {
                    Enums.EventType.CastVote, new UserDetailsBag(new List<UserDetails>(), DateTime.UtcNow)
                }
            };
        }

        /// <summary>
        /// Process user details based on timing.
        /// </summary>
        /// <param name="userDetailsBag">User details bag</param>
        /// <param name="elapsedTimeInSeconds">Time elapsed in seconds</param>
        /// <param name="waitTimeInSeconds">Wait time in seconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        private async Task ProcessUserDetailsBasedOnTiming(UserDetailsBag userDetailsBag, double elapsedTimeInSeconds, double waitTimeInSeconds, CancellationToken cancellationToken)
        {
            if (elapsedTimeInSeconds >= waitTimeInSeconds && userDetailsBag.UserDetailsCollection.Count > 0)
            {
                await this.ProcessMessage(userDetailsBag.UserDetailsCollection, cancellationToken);
                userDetailsBag.UserDetailsCollection.Clear();
                userDetailsBag.BatchStartTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Process message.
        /// </summary>
        /// <param name="userDetailsCollection">User details collection</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        private async Task ProcessMessage(List<UserDetails> userDetailsCollection, CancellationToken cancellationToken)
        {
            try
            {
                if(userDetailsCollection.Count > 0)
                {
                    await this.votingDatabaseMessageHandler.HandleMessage(userDetailsCollection);
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        /// <summary>
        /// Process msg.
        /// </summary>
        /// <param name="userDetailsBag">User details bag</param>
        /// <param name="userDetails">Newly arrived user details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task</returns>
        public async Task ProcessMsg(UserDetailsBag userDetailsBag, UserDetails userDetails, CancellationToken cancellationToken)
        {
            if ((DateTime.UtcNow - userDetailsBag.BatchStartTime).TotalSeconds <= this.votingDatabaseParameters.WaitTimeinSeconds.Seconds
                && userDetailsBag.UserDetailsCollection.Count < this.votingDatabaseParameters.SampleBatchSize)
            {
                /*
                   This code flow will be executed when we have a newly arrived msg and the batch is not full
                   and elapsed time is less than wait time.
                   So, in this case, we will add the msg to user details collection.
                */
                userDetailsBag.UserDetailsCollection.Add(userDetails);
            }
            else
            {
                /*
                   This code flow will be executed when we have a newly arrived message but either the wait time is over or
                   batch is full.
                   So, in this case, we will first process the collection, flush it, then add the newly arrived msg to
                   the collection.
                */
                await this.ProcessMessage(userDetailsBag.UserDetailsCollection, cancellationToken);
                userDetailsBag.UserDetailsCollection.Clear();
                userDetailsBag.UserDetailsCollection.Add(userDetails);
                userDetailsBag.BatchStartTime = DateTime.UtcNow;
            }
        }
    }
}