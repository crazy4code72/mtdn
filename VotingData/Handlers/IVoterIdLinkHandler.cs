namespace VotingData.Handlers
{
    using global::VotingData.Model;

    /// <summary>
    /// Voter id link handler interface.
    /// </summary>
    public interface IVoterIdLinkHandler
    {
        Enums.VoterIdLinkingStatus LinkVoterIdToAadhar(UserDetails userDetails);
    }
}