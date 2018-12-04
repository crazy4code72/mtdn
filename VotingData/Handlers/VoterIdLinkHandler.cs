using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using VotingData.Model;

namespace VotingData.Handlers
{
    public class VoterIdLinkHandler : IVoterIdLinkHandler
    {
        /// <summary>
        /// Verify otp.
        /// </summary>
        /// <param name="userDetails">User details</param>
        public bool LinkVoterIdToAadhar(UserDetails userDetails)
        {
            bool otpVerified = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(DataAccess.DataAccess.DbConnectionString))
                {
                    connection.Open();

                    var sqlCommand = new SqlCommand(DataAccess.DataAccess.LinkVoterIdToAadhar, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add parameters
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.AadharNo_Input, userDetails.AadharNo));

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            otpVerified = (bool)reader[DataAccess.DataAccess.OtpVerified_Output];
                        }
                    }
                }
            }
            catch (Exception)
            {
                return otpVerified;
            }

            return otpVerified;
        }
    }
}
