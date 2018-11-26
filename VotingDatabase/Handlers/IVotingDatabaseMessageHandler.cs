namespace VotingDatabase.Handlers
{
    using System.Threading.Tasks;
    using VotingData.Model;

    /// <summary>
    /// IVotingDatabaseMessageHandler interface.
    /// </summary>
    public interface IVotingDatabaseMessageHandler
    {
        Task HandleMessage(UserDetails userDetails);
    }
}