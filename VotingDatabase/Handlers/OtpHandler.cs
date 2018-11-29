﻿namespace VotingDatabase.Handlers
{
    using System;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using System.Data;
    using global::VotingDatabase.Model;

    /// <summary>
    /// Send otp handler class.
    /// </summary>
    internal class OtpHandler : IDataHandler
    {
        /// <summary>
        /// Voting database parameters.
        /// </summary>
        private readonly VotingDatabaseParameters votingDatabaseParameters;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="votingDatabaseParameters">Voting database parameters</param>
        public OtpHandler(VotingDatabaseParameters votingDatabaseParameters)
        {
            this.votingDatabaseParameters = votingDatabaseParameters;
        }

        /// <summary>
        /// Get contact details and send otp.
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        public async Task GetContactDetailsAndSendOtpForAadharNo(string aadharNo)
        {
            var otp = new Random().Next(1000, 9999);
            var contactDetails = UpdateOtpAndGetContactDetails(aadharNo, otp);

            var otpMessageForUser = string.Format("{0} is OTP for Matdaan. It will be invalid after 10 minutes.", otp);

            if (contactDetails.ContactNo != null)
            {
                await SendOtpToContactNo(contactDetails.ContactNo, otpMessageForUser);
            }

            if (contactDetails.EmailId != null)
            {
                await SendOtpToEmailId(contactDetails.EmailId, otpMessageForUser);
            }
        }

        /// <summary>
        /// Get contact no and email id.
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <param name="otp">Otp</param>
        /// <returns></returns>
        private UserDetails UpdateOtpAndGetContactDetails(string aadharNo, int otp)
        {
            // Make Db call, update the OTP for Aadhar No only if (aadhar no && contact no || email id), return the contact details.
            // SP called from here should also delete the OTP after 10 minutes.
            UserDetails userDetails = new UserDetails();
            try
            {
                using (SqlConnection connection = new SqlConnection(this.votingDatabaseParameters.DatabaseConnectionString))
                {
                    connection.Open();

                    var sqlCommand = new SqlCommand("UpdateOtpAndGetContactDetails", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add parameters
                    sqlCommand.Parameters.Add(new SqlParameter("@AADHAR_NO", aadharNo));
                    sqlCommand.Parameters.Add(new SqlParameter("@OTP", otp));

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            userDetails.ContactNo = (string)reader["CONTACT_NO"];
                            userDetails.EmailId = (string)reader["EMAIL_ID"];
                        }
                    }
                }
            }
            catch (Exception)
            {
                return userDetails;
            }
            return userDetails;
        }

        /// <summary>
        /// Send otp to contact no.
        /// </summary>
        /// <param name="contactNo">Contact no</param>
        /// <param name="otp">Otp</param>
        private async Task SendOtpToContactNo(string contactNo, string otp)
        {
            // Send otp to contact no.
        }

        /// <summary>
        /// Send otp to email id.
        /// </summary>
        /// <param name="emailId">Email id</param>
        /// <param name="otp">Otp</param>
        private async Task SendOtpToEmailId(string emailId, string otp)
        {
            // Send otp to email id.
        }
    }
}
