using VotingData.Model;

namespace VotingData.Handlers
{
    public interface IVoterIdLinkHandler
    {
        bool LinkVoterIdToAadhar(UserDetails userDetails);
    }
}