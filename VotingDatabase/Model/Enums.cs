namespace VotingDatabase.Model
{
    public static class Enums
    {
        public enum EventType
        {
            SendOtp = 1,
            VerifyOtp = 2,
            CastVote = 3
        }

        public enum Gender
        {
            Male = 1,
            Female = 2
        }
    }
}
