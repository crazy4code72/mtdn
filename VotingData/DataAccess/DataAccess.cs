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

        // Otp verified.
        public static string OtpVerified_Output = "OTP_VERIFIED";

        //TODO: Needs to be removed from here.
        // DB Connection string
        public const string DbConnectionString = "Data Source=localhost;Initial Catalog=Matdaan;User ID=AirWatchAdmin;Password=AirWatchAdmin;";
    }
}
