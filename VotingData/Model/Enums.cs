﻿namespace VotingData.Model
{
    public static class Enums
    {
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

        public enum ResponseMessageCode
        {
            Success = 200,
            Failure = 202
        }

        public enum VoterIdLinkingStatus
        {
            LinkingFailed = 201,
            AlreadyLinked = 202,
            SuccessfullyLinked = 200,
            Unauthorized = 203
        }
    }
}
