using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Fabric;
using System.Linq;
using global::VotingDatabase.Kafka;

namespace VotingDatabase
{
    public class VotingDatabaseParameters
    {
        /// <summary>
        /// Default wait time before flushing (3 seconds)
        /// </summary>
        private const int DefaultWaitTimeInSeconds = 3;

        /// <summary>
        /// Wait time in seconds.
        /// </summary>
        public TimeSpan WaitTimeinSeconds { get; set; }

        /// <summary>
        /// Default sample batch size (50)
        /// </summary>
        private const int DefaultSampleBatchSize = 50;

        /// <summary>
        /// Default sample wait time before flushing (2 minutes)
        /// </summary>
        private const int DefaultSampleWaitTimeInMinutes = 2;

        /// <summary>
        /// The default temp log file roll size.
        /// </summary>
        private const int DefaultTempLogFileRollSize = 20480;

        /// <summary>
        /// Default Autocommit interval (milliseconds) for message pointers (5000)
        /// </summary>
        private const int DefaultAutoCommitInterval = 5000;

        /// <summary>
        /// Default byte message(s) retrieval size (10485760)
        /// </summary>
        private const long DefaultMaxFetchByteSize = 10485760;

        /// <summary>
        /// Default poll interval in milliseconds (100)
        /// </summary>
        private const int DefaultPollIntervalInMilliseconds = 100;

        /// <summary>
        /// Default max concurrent threads for dispatching to actors (10)
        /// </summary>
        private const int DefaultMaxConcurrentProcessors = 10;

        /// <summary>
        /// Default security settings for kafka (false)
        /// </summary>
        private const bool DefaultUseSecureKafka = false;

        /// <summary>
        /// The default value for is client logging enabled.
        /// </summary>
        private const bool DefaultIsClientLoggingEnabled = false;

        /// <summary>
        /// The application sample consumer config section.
        /// </summary>
        private const string DatabaseConsumerConfigSection = "DatabaseConsumerConfigSection";

        /// <summary>
        /// The config.
        /// </summary>
        private const string ServiceFabricConfig = "Config";

        /// <summary>
        /// Gets or sets the database connection string.
        /// </summary>
        public string DatabaseConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the auto commit interval in milliseconds.
        /// </summary>
        public int KafkaAutoCommitIntervalInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the auto offset reset.
        /// </summary>
        public string KafkaAutoOffsetReset { get; set; }

        /// <summary>
        /// Gets or sets the consumer group id.
        /// </summary>
        public string KafkaConsumerGroupId { get; set; }

        /// <summary>
        /// Gets or sets the server address list.
        /// </summary>
        public List<string> KafkaListenerServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the kafka producer server address.
        /// </summary>
        public List<string> KafkaProducerServerAddress { get; set; }

        /// <summary>
        /// Gets or sets the kafka producer topic name.
        /// </summary>
        public string KafkaProducerTopicName { get; set; }

        /// <summary>
        /// Gets or sets the topic name.
        /// </summary>
        public string KafkaListenerTopicName { get; set; }

        /// <summary>
        /// Gets or sets the max partition fetch byte size.
        /// </summary>
        public long? KafkaMaxPartitionFetchBytes { get; set; }

        /// <summary>
        /// Gets or sets the max number of concurrent message processors.
        /// </summary>
        public int MaxNumberOfConcurrentMessageProcessors { get; set; }

        /// <summary>
        /// Gets or sets the message poll interval.
        /// </summary>
        public TimeSpan MessagePollIntervalInMilliseconds { get; set; }

        /// <summary>
        /// Gets or sets the sample batch size.
        /// </summary>
        public int SampleBatchSize { get; set; }

        /// <summary>
        /// Gets or sets the sample wait time in minutes.
        /// </summary>
        public TimeSpan SampleWaitTimeInMinutes { get; set; }

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
        /// Use kafka security settings, or use insecure kafka settings.
        /// </summary>
        public bool UseSecureKafka { get; set; }

        /// <summary>
        /// Gets or sets the logging level.
        /// </summary>
        public SourceLevels LoggingLevel { get; set; }

        /// <summary>
        /// Gets or sets the log file roll size.
        /// </summary>
        public int LogFileRollSize { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether environment name.
        /// </summary>
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets the logging path.
        /// </summary>
        public string LoggingPath { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is kafka client logging enabled.
        /// </summary>
        public bool IsKafkaClientLoggingEnabled { get; set; }

        /// <summary>
        /// Gets the application parameters from the activation context.   Note that we cannot continue if there is an error here
        /// so the exception will cause the service to be unable to start, and that is the desired result.
        /// </summary>
        /// <param name="activationContext">The activation Context.</param>
        /// <returns>Application Parameters</returns>
        public static VotingDatabaseParameters GetDatabaseConsumerParameters(ICodePackageActivationContext activationContext)
        {
            var configurationPackage = activationContext.GetConfigurationPackageObject(ServiceFabricConfig);
            var consumerTopicName = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaListenerTopicName)].Value;
            var listenerServerAddressList = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaListenerServerAddress)].Value.Split(',').ToList();
            var producerServerAddressList = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaProducerServerAddress)].Value.Split(',').ToList();
            var producerTopicName = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaProducerTopicName)].Value;
            var consumerGroupId = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaConsumerGroupId)].Value;
            var autoCommitIntervalInMilliseconds = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaAutoCommitIntervalInMilliseconds)].Value;
            var autoOffsetReset = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaAutoOffsetReset)].Value;
            var maxPartitionFetchByteSize = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KafkaMaxPartitionFetchBytes)].Value;
            var messagePollIntervalInMilliseconds = configurationPackage.Settings
                .Sections[DatabaseConsumerConfigSection].Parameters[nameof(MessagePollIntervalInMilliseconds)].Value;
            var maxNumberOfConcurrentMessageProcessors = configurationPackage.Settings
                .Sections[DatabaseConsumerConfigSection].Parameters[nameof(MaxNumberOfConcurrentMessageProcessors)]
                .Value;
            var sampleBatchSize = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(SampleBatchSize)].Value;
            var sampleWaitTimeInMinutes = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(SampleWaitTimeInMinutes)].Value;

            var databaseConnection = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(DatabaseConnectionString)];
            var databaseConnectionString = databaseConnection.Value;
            if (databaseConnection.IsEncrypted)
            {
                // databaseConnectionString = databaseConnection.DecryptValue().ConvertToUnsecureString();
            }

            var waitTimeInSeconds = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(WaitTimeinSeconds)].Value;
            var certificateLocation = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(CertificateLocation)].Value;
            var keyLocation = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KeyLocation)].Value;
            var securityProtocol = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(SecurityProtocol)].Value;

            var keyPasswordProp = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(KeyPassword)];
            var keyPassword = keyPasswordProp.Value;
            if (keyPasswordProp.IsEncrypted)
            {
                //keyPassword = keyPasswordProp.DecryptValue().ConvertToUnsecureString();
            }

            var certificateAuthorityLocation = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(CertificateAuthorityLocation)].Value;
            var useSecureKafkaValue = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(UseSecureKafka)].Value;

            var environmentName = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(EnvironmentName)].Value;

            var loggingPath = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(LoggingPath)].Value;

            if (!int.TryParse(autoCommitIntervalInMilliseconds, out int autoCommitInterval))
            {
                autoCommitInterval = DefaultAutoCommitInterval;
            }

            if (!long.TryParse(maxPartitionFetchByteSize, out long maxFetchByteSize))
            {
                maxFetchByteSize = DefaultMaxFetchByteSize;
            }

            if (!int.TryParse(messagePollIntervalInMilliseconds, out int pollIntervalInMilliseconds))
            {
                pollIntervalInMilliseconds = DefaultPollIntervalInMilliseconds;
            }

            if (!int.TryParse(maxNumberOfConcurrentMessageProcessors, out int maxConcurrentProcessors))
            {
                maxConcurrentProcessors = DefaultMaxConcurrentProcessors;
            }

            if (!int.TryParse(sampleBatchSize, out int tempSampleBatchSize))
            {
                tempSampleBatchSize = DefaultSampleBatchSize;
            }

            if (!int.TryParse(waitTimeInSeconds, out int tempWaitTimeInSeconds))
            {
                tempWaitTimeInSeconds = DefaultWaitTimeInSeconds;
            }

            if (!int.TryParse(sampleWaitTimeInMinutes, out int tempSampleWaitTimeInMinutes))
            {
                tempSampleWaitTimeInMinutes = DefaultSampleWaitTimeInMinutes;
            }

            if (!bool.TryParse(useSecureKafkaValue, out bool useSecureKafka))
            {
                useSecureKafka = DefaultUseSecureKafka;
            }

            string loggingLevel = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(LoggingLevel)].Value;

            if (!Enum.TryParse(loggingLevel, true, out SourceLevels sourceLevels))
            {
                sourceLevels = SourceLevels.Information;
            }

            string logFileRollSize = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(LogFileRollSize)].Value;

            if (!int.TryParse(logFileRollSize, out int tempLogFileRollSize))
            {
                tempLogFileRollSize = DefaultTempLogFileRollSize;
            }

            var isClientLoggingEnabledValue = configurationPackage.Settings.Sections[DatabaseConsumerConfigSection]
                .Parameters[nameof(IsKafkaClientLoggingEnabled)].Value;

            if (!bool.TryParse(isClientLoggingEnabledValue, out bool isClientLoggingEnabled))
            {
                isClientLoggingEnabled = DefaultIsClientLoggingEnabled;
            }

            var result = new VotingDatabaseParameters
            {
                KafkaAutoCommitIntervalInMilliseconds = autoCommitInterval,
                KafkaAutoOffsetReset = autoOffsetReset,
                KafkaListenerServerAddress = listenerServerAddressList,
                KafkaListenerTopicName = consumerTopicName,
                KafkaConsumerGroupId = consumerGroupId,
                KafkaMaxPartitionFetchBytes = maxFetchByteSize,
                MessagePollIntervalInMilliseconds = new TimeSpan(0, 0, 0, 0, pollIntervalInMilliseconds),
                MaxNumberOfConcurrentMessageProcessors = maxConcurrentProcessors,
                DatabaseConnectionString = databaseConnectionString,
                SampleWaitTimeInMinutes = new TimeSpan(0, tempSampleWaitTimeInMinutes, 0),
                WaitTimeinSeconds = new TimeSpan(0, 0, tempWaitTimeInSeconds),
                SampleBatchSize = tempSampleBatchSize,
                KafkaProducerServerAddress = producerServerAddressList,
                KafkaProducerTopicName = producerTopicName,
                CertificateLocation = certificateLocation,
                KeyLocation = keyLocation,
                SecurityProtocol = securityProtocol,
                KeyPassword = keyPassword,
                CertificateAuthorityLocation = certificateAuthorityLocation,
                UseSecureKafka = useSecureKafka,
                EnvironmentName = environmentName,
                LoggingPath = loggingPath,
                LoggingLevel = sourceLevels,
                LogFileRollSize = tempLogFileRollSize,
                IsKafkaClientLoggingEnabled = isClientLoggingEnabled
            };

            return result;
        }

    }
}
