namespace VotingDatabase.DataAccess
{
    /// <summary>
    /// Data access.
    /// </summary>
    internal static class DataAccess
    {
        // SP names.
        public static string UpdateOtpAndGetContactDetails = "UpdateOtpAndGetContactDetails";
        public static string CastVote = "CastVote";

        // DB input parameters.
        public static string AadharNo_Input = "@AADHAR_NO";
        public static string Otp_Input = "@OTP";
        public static string VoterId_Input = "@VOTER_ID";
        public static string VoteFor_Input = "@VOTE_FOR";

        // DB output parameters.
        public static string ContactNo_Output = "CONTACT_NO";
        public static string EmailId_Output = "EMAIL_ID";
        public static string AadharNo_Output = "AADHAR_NO";
    }
}
