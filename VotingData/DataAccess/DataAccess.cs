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
        public static string VoterId_Input = "@VOTER_ID";
        public static string Name_Input = "@NAME";
        public static string Dob_Input = "@DOB";
        public static string FatherName_Input = "@FATHER_NAME";
        public static string Gender_Input = "@GENDER";

        // DB output parameters.
        public static string ContactNo_Output = "CONTACT_NO";
        public static string EmailId_Output = "EMAIL_ID";
        public static string VoterIdLinkingStatus_Output = "VOTER_ID_LINKING_STATUS";

        // Otp verified.
        public static string OtpVerified_Output = "OTP_VERIFIED";

        //TODO: Needs to be removed from here.
        // DB Connection string
        public const string DbConnectionString = "Data Source=localhost;Initial Catalog=Matdaan;User ID=AirWatchAdmin;Password=AirWatchAdmin;";
    }
}
