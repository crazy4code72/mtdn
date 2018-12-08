﻿namespace VotingDatabase.Handlers
{
    using global::VotingDatabase.DataAccess;
    using global::VotingDatabase.Model;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;

    /// <summary>
    /// Send otp handler class.
    /// </summary>
    internal class CastVoteHandler : IDataHandler
    {
        /// <summary>
        /// Voting database parameters.
        /// </summary>
        private readonly VotingDatabaseParameters votingDatabaseParameters;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="votingDatabaseParameters">Voting database parameters</param>
        public CastVoteHandler(VotingDatabaseParameters votingDatabaseParameters)
        {
            this.votingDatabaseParameters = votingDatabaseParameters;
        }

        /// <summary>
        /// Cast vote.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>User details</returns>
        public async Task CastVote(UserDetails userDetails)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(this.votingDatabaseParameters.DatabaseConnectionString))
                {
                    connection.Open();

                    var sqlCommand = new SqlCommand(DataAccess.CastVote, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add parameters
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.AadharNo_Input, userDetails.AadharNo));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.VoterId_Input, userDetails.VoterId));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.VoteFor_Input, userDetails.VoteFor));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.Otp_Input, userDetails.Otp));

                    sqlCommand.ExecuteNonQuery();
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}