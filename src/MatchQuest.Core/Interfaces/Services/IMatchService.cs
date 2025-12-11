using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services
{
    public interface IMatchService
    {
        List<User> GetAllMatchesFromUserId(int userId);
        Match? CreateIfNotExists(int userId1, int userId2);
    }
}
