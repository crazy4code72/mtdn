namespace VotingDatabase.DataAccess
{
    /// <summary>
    /// Data access.
    /// </summary>
    public static class DataAccess
    {
        // SP names.
        public static string UpdateOtpAndGetContactDetails = "UpdateOtpAndGetContactDetails";

        // DB input parameters.
        public static string AadharNo_Input = "@AADHAR_NO";
        public static string Otp_Input = "@OTP";

        // DB output parameters.
        public static string ContactNo_Output = "CONTACT_NO";
        public static string EmailId_Output = "EMAIL_ID";
    }
}
