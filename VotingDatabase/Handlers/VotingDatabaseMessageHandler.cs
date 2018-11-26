﻿namespace VotingDatabase.Handlers
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
        /// Handler Factory
        /// </summary>
        private readonly Func<Enums.EventType, IDataHandler> handlerFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="VotingDatabaseMessageHandler"/> class.
        /// </summary>
        public VotingDatabaseMessageHandler(Func<Enums.EventType, IDataHandler> handlerFactory)
        {
            this.handlerFactory = handlerFactory;
        }

        /// <summary>
        /// Handle incoming messages.
        /// </summary>
        /// <param name="userDetails">User details.</param>
        /// <returns>Task</returns>
        public async Task HandleMessage(UserDetails userDetails)
        {
            IDataHandler handler = handlerFactory(userDetails.EventType);
            switch (userDetails.EventType)
            {
                case Enums.EventType.SendOtp: await ((OtpHandler)handler).GetContactDetailsAndSendOtpForAadharNo(userDetails.AadharNo); break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
    }
}
