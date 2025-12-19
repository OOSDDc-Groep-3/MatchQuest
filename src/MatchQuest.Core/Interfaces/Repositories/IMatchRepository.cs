using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories
{
    public interface IMatchRepository
    {
        List<User> GetAllMatchesFromUserId(int userId);
        Match? GetByUserIds(int userId, int userId2);

        Match? CreateMatch(int userId1, int userId2);
    }
}
