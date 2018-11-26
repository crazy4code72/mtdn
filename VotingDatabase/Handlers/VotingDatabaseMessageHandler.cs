namespace VotingDatabase.Handlers
{
    using System;
    using System.Threading.Tasks;
    using VotingData.Model;

    /// <summary>
    /// Voting database message handler class.
    /// </summary>
    internal class VotingDatabaseMessageHandler : IVotingDatabaseMessageHandler
    {
        /// <summary>
        /// The database consumer parameters.
        /// </summary>
        private readonly VotingDatabaseParameters votingDatabaseParameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="VotingDatabaseMessageHandler"/> class.
        /// </summary>
        public VotingDatabaseMessageHandler(VotingDatabaseParameters votingDatabaseParameters)
        {
            this.votingDatabaseParameters = votingDatabaseParameters;
        }

        /// <summary>
        /// Handler incoming messages.
        /// </summary>
        /// <param name="userDetails">User details.</param>
        /// <returns>Task</returns>
        public async Task HandleMessage(UserDetails userDetails)
        {
            switch (userDetails.EventType)
            {
                case Enums.EventType.SendOtp:
                    // TODO: Use factory for this.
                    var sendOtpHandler = new SendOtpHandler(this.votingDatabaseParameters);
                    await sendOtpHandler.GetContactDetailsAndSendOtpForAadharNo(userDetails.AadharNo);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
