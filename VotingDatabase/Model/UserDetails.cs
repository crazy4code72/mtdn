namespace VotingDatabase.Model
{
    /// <summary>
    /// User details.
    /// </summary>
    public class UserDetails
    {
        public string AadharNo { get; set; }

        public string VoterId { get; set; }

        public string Name { get; set; }

        public string DOB { get; set; }

        public string FatherName { get; set; }

        public bool IsVoterIdLinkedWithAadhar { get; set; }

        public string VoteFor { get; set; }

        public Enums.Gender Gender { get; set; }

        public Enums.EventType EventType { get; set; }

        public string ContactNo { get; set; }

        public string EmailId { get; set; }

        public int Otp { get; set; }
    }
}
