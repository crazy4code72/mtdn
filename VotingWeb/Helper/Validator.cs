using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VotingWeb.Model;

namespace VotingWeb.Helper
{
    /// <summary>
    /// Validator.
    /// </summary>
    internal static class Validator
    {
        /// <summary>
        /// Validate aadhar no to send otp
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <returns>Action result</returns>
        internal static IActionResult ValidateAadharNoToSendOtp(string aadharNo)
        {
            double.TryParse(aadharNo, out double result);
            if (string.IsNullOrWhiteSpace(aadharNo) || !aadharNo.Trim().Length.Equals(12) || result.Equals(0))
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.ResponseMessageCode.Failure.ToString()
                };
            }
            return null;
        }

        /// <summary>
        /// Validate otp verification.
        /// </summary>
        /// <param name="aadharNo">Aadhar no</param>
        /// <param name="otp">Otp</param>
        /// <returns>Action result</returns>
        internal static IActionResult ValidateVerifyOtpData(string aadharNo, string otp)
        {
            double.TryParse(aadharNo, out double aadharResult);
            int.TryParse(otp, out int otpResult);
            if (string.IsNullOrWhiteSpace(aadharNo) || !aadharNo.Trim().Length.Equals(12) || aadharResult.Equals(0) ||
                string.IsNullOrWhiteSpace(otp) || !otp.Trim().Length.Equals(6) || otpResult.Equals(0))
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.ResponseMessageCode.Failure.ToString()
                };
            }
            return null;
        }

        /// <summary>
        /// Validate link voter id to aadhar data.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        internal static IActionResult ValidateLinkVoterIdToAadharData(UserDetails userDetails)
        {
            double.TryParse(userDetails.AadharNo, out double aadharResult);
            if (string.IsNullOrWhiteSpace(userDetails.AadharNo) || !userDetails.AadharNo.Trim().Length.Equals(12) || aadharResult.Equals(0) ||
                string.IsNullOrWhiteSpace(userDetails.VoterId) || !userDetails.VoterId.Trim().Length.Equals(10) ||
                string.IsNullOrWhiteSpace(userDetails.Name) || string.IsNullOrWhiteSpace(userDetails.FatherName) ||
                string.IsNullOrWhiteSpace(userDetails.DOB) || !userDetails.DOB.Trim().Length.Equals(10) ||
                !Enum.IsDefined(typeof(Enums.Gender), userDetails.Gender) ||
                userDetails.Otp.Equals(null) || userDetails.Otp < 100000 || userDetails.Otp > 999999)
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.VoterIdLinkingStatus.Unauthorized.ToString()
                };
            }
            return null;
        }

        /// <summary>
        /// Validate cast vote data.
        /// </summary>
        /// <param name="userDetails">User details</param>
        /// <returns>Action result</returns>
        internal static IActionResult ValidateCastVoteData(UserDetails userDetails)
        {
            double.TryParse(userDetails.AadharNo, out double aadharResult);
            if (string.IsNullOrWhiteSpace(userDetails.AadharNo) || !userDetails.AadharNo.Trim().Length.Equals(12) || aadharResult.Equals(0) ||
                string.IsNullOrWhiteSpace(userDetails.VoterId) || !userDetails.VoterId.Trim().Length.Equals(10) ||
                string.IsNullOrWhiteSpace(userDetails.VoteFor) ||
                userDetails.Otp.Equals(null) || userDetails.Otp < 100000 || userDetails.Otp > 999999)
            {
                return new ContentResult
                {
                    StatusCode = (int)Enums.ResponseMessageCode.Success,
                    Content = Enums.CastVoteStatus.VotingFailed.ToString()
                };
            }
            return null;
        }
    }
}
