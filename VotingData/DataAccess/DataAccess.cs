namespace VotingData.DataAccess
{
    /// <summary>
    /// Data access.
    /// </summary>
    internal static class DataAccess
    {
        // SP names.
        public static string UpdateOtpAndGetContactDetails = "UpdateOtpAndGetContactDetails";
        public static string VerifyOtp = "VerifyOtp";
        public static string LinkVoterIdToAadhar = "LinkVoterIdToAadhar";

        // DB input parameters.
        public static string AadharNo_Input = "@AADHAR_NO";
        public static string Otp_Input = "@OTP";

        // DB output parameters.
        public static string ContactNo_Output = "CONTACT_NO";
        public static string EmailId_Output = "EMAIL_ID";

        // No of rows affected.
        public static string NoOfRowsAffected_Output = "NO_OF_ROWS_AFFECTED";

        //TODO: Needs to be removed from here.
        // DB Connection string
        public const string DbConnectionString = "Data Source=localhost;Initial Catalog=Matdaan;User ID=AirWatchAdmin;Password=AirWatchAdmin;";
    }
}
