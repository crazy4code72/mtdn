namespace VotingData.Handlers
{
    public interface IOtpVerificationHandler
    {
        bool VerifyOtp(string aadharNo, string userEnteredOtp);
    }
}