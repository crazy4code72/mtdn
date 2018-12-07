namespace VotingWeb.Helper
{
    using global::VotingWeb.Model;
    using Microsoft.AspNetCore.Mvc;
    using System.Net.Http;

    /// <summary>
    /// Action result creator
    /// </summary>
    public static class ActionResultCreator
    {
        /// <summary>
        /// Create action result
        /// </summary>
        /// <param name="response">Http response</param>
        /// <returns>Action result</returns>
        internal static IActionResult CreateActionResult(HttpResponseMessage response)
        {
            return new ContentResult
            {
                StatusCode = (int)Enums.ResponseMessageCode.Success,
                Content = (int)response.StatusCode == (int)Enums.ResponseMessageCode.Success
                    ? Enums.ResponseMessageCode.Success.ToString()
                    : Enums.ResponseMessageCode.Failure.ToString()
            };
        }

        /// <summary>
        /// Create action result after trying to link voter id with aadhar.
        /// </summary>
        /// <param name="response">Http response</param>
        /// <returns>Action result</returns>
        internal static IActionResult CreateVoterIdLinkActionResult(HttpResponseMessage response)
        {
            string voterIdLinkStatus;
            switch ((int)response.StatusCode)
            {
                case (int)Enums.VoterIdLinkingStatus.Unauthorized: voterIdLinkStatus = Enums.VoterIdLinkingStatus.Unauthorized.ToString(); break;
                case (int)Enums.VoterIdLinkingStatus.LinkingFailed: voterIdLinkStatus = Enums.VoterIdLinkingStatus.LinkingFailed.ToString(); break;
                case (int)Enums.VoterIdLinkingStatus.AlreadyLinked: voterIdLinkStatus = Enums.VoterIdLinkingStatus.AlreadyLinked.ToString(); break;
                case (int)Enums.VoterIdLinkingStatus.SuccessfullyLinked: voterIdLinkStatus = Enums.VoterIdLinkingStatus.SuccessfullyLinked.ToString(); break;
                default: voterIdLinkStatus = Enums.VoterIdLinkingStatus.Unauthorized.ToString(); break;
            }

            return new ContentResult
            {
                StatusCode = (int)Enums.ResponseMessageCode.Success,
                Content = voterIdLinkStatus
            };
        }

        /// <summary>
        /// Create action result after trying to cast vote.
        /// </summary>
        /// <param name="response">Http response</param>
        /// <returns>Action result</returns>
        internal static IActionResult CreateCastVoteActionResult(HttpResponseMessage response)
        {
            string castVoteStatus;
            switch ((int)response.StatusCode)
            {
                case (int)Enums.CastVoteStatus.VotingFailed: castVoteStatus = Enums.CastVoteStatus.VotingFailed.ToString(); break;
                case (int)Enums.CastVoteStatus.AlreadyVoted: castVoteStatus = Enums.CastVoteStatus.AlreadyVoted.ToString(); break;
                case (int)Enums.CastVoteStatus.SuccessfullyVoted: castVoteStatus = Enums.CastVoteStatus.SuccessfullyVoted.ToString(); break;
                default: castVoteStatus = Enums.CastVoteStatus.VotingFailed.ToString(); break;
            }

            return new ContentResult
            {
                StatusCode = (int)Enums.ResponseMessageCode.Success,
                Content = castVoteStatus
            };
        }
    }
}
