namespace VotingWeb.Model
{
    using System;

    /// <summary>
    /// Enum for parameter to throttle on.
    /// </summary>
    public enum ThrottleOn
    {
        IpAddress = 1,
        Path = 2
    }

    /// <summary>
    /// Throttle info.
    /// </summary>
    public class ThrottleInfo
    {
        /// <summary>
        /// Expires at.
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Request count.
        /// </summary>
        public int RequestCount { get; set; }
    }
}