namespace VotingDatabase.Handlers
{
    using global::VotingDatabase.DataAccess;
    using global::VotingDatabase.Model;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Twilio;

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
        /// Randomizer.
        /// </summary>
        private static readonly Random randomizer = new Random();

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
        /// <param name="userDetails">User details</param>
        public async Task GetContactDetailsAndSendOtpForAadharNo(List<UserDetails> userDetails)
        {
            userDetails.ForEach(ud => ud.Otp = randomizer.Next(100000, 999999));
            var contactDetails = UpdateOtpAndGetContactDetails(userDetails);

            foreach (var contactDetail in contactDetails)
            {
                var otpMessageForUser = string.Concat(contactDetail.Otp, " is OTP for Aadhar verification.");

                if (contactDetail.ContactNo != null)
                {
                    // Commented for now since I have not created a Twilio account.
                    // await SendOtpToContactNo(contactDetail.ContactNo, otpMessageForUser);
                }

                if (contactDetail.EmailId != null)
                {
                    SendOtpToEmailId(contactDetail.EmailId, otpMessageForUser).Wait();
                }
            }
        }

        /// <summary>
        /// Get contact no and email id.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>User details</returns>
        private List<UserDetails> UpdateOtpAndGetContactDetails(List<UserDetails> userDetails)
        {
            // Make Db call, update the OTP for Aadhar No only if (aadhar no && contact no || email id), return the contact details.
            // SP called from here should also delete the OTP after 10 minutes.
            try
            {
                var userDetailsTable = new DataTable();
                userDetailsTable.Columns.Add(DataAccess.AadharNo_Input, typeof(string));
                userDetailsTable.Columns.Add(DataAccess.Otp_Input, typeof(int));

                userDetails.ForEach(ud => userDetailsTable.Rows.Add(ud.AadharNo, ud.Otp));

                using (SqlConnection connection = new SqlConnection(this.votingDatabaseParameters.DatabaseConnectionString))
                {
                    connection.Open();

                    var sqlCommand = new SqlCommand(DataAccess.UpdateOtpAndGetContactDetails, connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    // Add table-valued parameters
                    SqlParameter parameter = sqlCommand.Parameters.AddWithValue("@OtpForAadharNo", userDetailsTable);
                    parameter.SqlDbType = SqlDbType.Structured;

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            foreach(var userDetail in userDetails)
                            {
                                if (userDetail.AadharNo.Equals((string)reader[DataAccess.AadharNo_Output]))
                                {
                                    userDetail.ContactNo = (string)reader[DataAccess.ContactNo_Output];
                                    userDetail.EmailId = (string)reader[DataAccess.EmailId_Output];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // ignored
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
            try
            {
                var twilioRestClient =
                    new TwilioRestClient(Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID"), Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN"));

                twilioRestClient.SendMessage("+15627356095", contactNo, otp);
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// Send Otp to email id.
        /// </summary>
        private static async Task SendOtpToEmailId(string emailId, string otp)
        {
            try
            {
                var apiKey = Environment.GetEnvironmentVariable("SENDGRID_KEY");
                var client = new SendGridClient(apiKey);
                var from = new EmailAddress("aadhar@gov.in", "Aadhar");
                const string subject = "OTP for Aadhar verification";
                var to = new EmailAddress(emailId, string.Empty);
                var htmlContent = string.Empty;
                var msg = MailHelper.CreateSingleEmail(from, to, subject, otp, htmlContent);
                await client.SendEmailAsync(msg);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
