namespace VotingWeb.Model
{
    public static class Enums
    {
        public enum ResponseMessageCode
        {
            Success = 200,
            Failure = 202
        }

        public enum EventType
        {
            SendOtp = 1,
            VerifyOtp = 2
        }

        public enum Gender
        {
            Male = 1,
            Female = 2
        }
    }
}
