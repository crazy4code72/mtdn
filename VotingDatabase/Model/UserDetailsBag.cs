namespace VotingDatabase.Model
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// User details bag.
    /// </summary>
    public class UserDetailsBag
    {
        internal UserDetailsBag(List<UserDetails> userDetailsCollection, DateTime batchStartTime)
        {
            this.UserDetailsCollection = userDetailsCollection;
            this.BatchStartTime = batchStartTime;
        }

        /// <summary>
        /// User details collection.
        /// </summary>
        public List<UserDetails> UserDetailsCollection { get; set; }

        /// <summary>
        /// Batch start time.
        /// </summary>
        public DateTime BatchStartTime { get; set; }
    }
}