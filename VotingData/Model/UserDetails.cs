namespace VotingData.Model
{
    using Newtonsoft.Json;

    /// <summary>
    /// User details.
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class UserDetails
    {
        [JsonProperty]
        public string AadharNo { get; set; }

        [JsonProperty]
        public string VoterId { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string DOB { get; set; }

        [JsonProperty]
        public string FatherName { get; set; }

        [JsonProperty]
        public bool IsVoterIdLinkedWithAadhar { get; set; }

        [JsonProperty]
        public string VoteFor { get; set; }

        [JsonProperty]
        public Enums.Gender Gender { get; set; }

        [JsonProperty]
        public Enums.EventType EventType { get; set; }

        [JsonProperty]
        public string ContactNo { get; set; }

        [JsonProperty]
        public string EmailId { get; set; }

        [JsonProperty]
        public int Otp { get; set; }
    }
}
