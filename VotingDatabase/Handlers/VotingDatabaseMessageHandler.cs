﻿namespace VotingDatabase.Handlers
{
    using global::VotingDatabase.Model;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
        public async Task HandleMessage(List<UserDetails> userDetails)
        {
            IDataHandler handler = handlerFactory(userDetails.First().EventType);
            switch (userDetails.First().EventType)
            {
                case Enums.EventType.SendOtp: await ((OtpHandler)handler).GetContactDetailsAndSendOtpForAadharNo(userDetails); break;
                case Enums.EventType.CastVote: await ((CastVoteHandler)handler).CastVote(userDetails); break;
            }
        }
    }
}
