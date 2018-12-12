namespace VotingDatabase.Handlers
{
    using global::VotingDatabase.DataAccess;
    using global::VotingDatabase.Model;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Linq;
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
        public async Task CastVote(List<UserDetails> userDetails)
        {
            try
            {
                var userDetailsTable = new DataTable();
                userDetailsTable.Columns.Add(DataAccess.AadharNo_Input, typeof(string));
                userDetailsTable.Columns.Add(DataAccess.VoterId_Input, typeof(string));
                userDetailsTable.Columns.Add(DataAccess.VoteFor_Input, typeof(string));
                userDetailsTable.Columns.Add(DataAccess.Otp_Input, typeof(int));

                userDetails.ForEach(ud => userDetailsTable.Rows.Add(ud.AadharNo, ud.VoterId, ud.VoteFor, ud.Otp));

                using (SqlConnection connection = new SqlConnection(this.votingDatabaseParameters.DatabaseConnectionString))
                {
                    connection.Open();

                    var sqlCommand = new SqlCommand(DataAccess.CastVote, connection) { CommandType = CommandType.StoredProcedure };

                    // Add table-valued parameters
                    SqlParameter parameter = sqlCommand.Parameters.AddWithValue(DataAccess.CastVote_DataType, userDetailsTable);
                    parameter.SqlDbType = SqlDbType.Structured;

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
