﻿namespace VotingWeb.Model
{
    public static class Enums
    {
        public enum ResponseMessageCode
        {
            Success = 200,
            Failure = 202,
            TooManyTries = 201
        }

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

        public enum VoterIdLinkingStatus
        {
            LinkingFailed = 201,
            AlreadyLinked = 202,
            SuccessfullyLinked = 200,
            Unauthorized = 203
        }

        public enum CastVoteStatus
        {
            SuccessfullyVoted = 200,
            AlreadyVoted = 202,
            VotingFailed = 203
        }
    }
}
