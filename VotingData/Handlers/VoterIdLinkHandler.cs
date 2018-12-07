namespace VotingData.Handlers
{
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using global::VotingData.Model;

    /// <summary>
    /// Voter id link handler.
    /// </summary>
    public class VoterIdLinkHandler : IVoterIdLinkHandler
    {
        /// <summary>
        /// Link Voter id to aadhar.
        /// </summary>
        /// <param name="userDetails">User details</param>
        public Enums.VoterIdLinkingStatus LinkVoterIdToAadhar(UserDetails userDetails)
        {
            Enums.VoterIdLinkingStatus voterIdLinkingStatus = Enums.VoterIdLinkingStatus.Unauthorized;
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
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.VoterId_Input, userDetails.VoterId));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.Name_Input, userDetails.Name));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.Dob_Input, userDetails.DOB));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.FatherName_Input, userDetails.FatherName));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.Gender_Input, userDetails.Gender.ToString()));
                    sqlCommand.Parameters.Add(new SqlParameter(DataAccess.DataAccess.Otp_Input, userDetails.Otp));

                    using (var reader = sqlCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var value = (string)reader[DataAccess.DataAccess.VoterIdLinkingStatus_Output];
                            voterIdLinkingStatus = (Enums.VoterIdLinkingStatus)Enum.Parse(typeof(Enums.VoterIdLinkingStatus), value);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return voterIdLinkingStatus;
            }

            return voterIdLinkingStatus;
        }
    }
}
