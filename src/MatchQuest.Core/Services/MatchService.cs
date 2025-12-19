using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        public MatchService(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public List<User> GetAllMatchesFromUserId(int userId)
        {
            return _matchRepository.GetAllMatchesFromUserId(userId);
        }

        public Match? CreateIfNotExists(int userId1, int userId2)
        {
            var exists = (_matchRepository.GetByUserIds(userId1, userId2) != null);

            if (!exists)
            {
                return _matchRepository.CreateMatch(userId1, userId2);
            }

            return null;
        }
    }
}
