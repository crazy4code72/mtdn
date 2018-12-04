using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using VotingData.Model;
using VotingData.DataAccess;
using VotingDatabase;

namespace VotingData.Handlers
{
    public class OtpVerificationHandler : IOtpVerificationHandler
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="votingDatabaseParameters"></param>
        //public OtpVerificationHandler(VotingDatabaseParameters votingDatabaseParameters)
        //{
        //    this.votingDatabaseParameters = votingDatabaseParameters;
        //}

        /// <summary>
        /// Verify otp.
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <param name="userEnteredOtp">User entered otp</param>
        public bool VerifyOtp(string aadharNo, string userEnteredOtp)
        {
            bool otpVerified = false;
            try
            {
                using (SqlConnection connection = new SqlConnection(DataAccess.DataAccess.DbConnectionString))
                {
                    connection.Open();

                    var sqlCommand = new SqlCommand(DataAccess.DataAccess.VerifyOtp, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add parameters
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.AadharNo_Input, aadharNo));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.Otp_Input, userEnteredOtp));

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
