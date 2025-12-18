using System.Diagnostics;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IGameService _gameService;
        private readonly IReactionService _reactionService;
        public UserService(IUserRepository userRepository, IGameService gameService, IReactionService reactionService)
        {
            _userRepository = userRepository;
            _gameService = gameService;
            _reactionService = reactionService;
        }

        public User? Get(string email)
        {
            var result = _userRepository.Get(email);
            return result;
        }

        public User? Get(int id)
        {
            var result = _userRepository.Get(id);
            return result;
        }

        public List<User> GetAll()
        {
            var clients = _userRepository.GetAll();
            return clients;
        }

        public User? Create(User client)
        {
            try
            {
                var created = _userRepository.Add(client);
                return created;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ClientService.Create: Exception: {ex}");
                return null;
            }
        }
        
        public List<MatchingScore> GetUserMatchPool(User matcher, GameType? gameType)
        {
            try
            {
                var matchPool = new List<MatchingScore>();
                
                var gameTypeValue = gameType.HasValue ? (int)gameType.Value : -1;
                
                var users = _userRepository.GetUsersWithMatchingGameType(matcher.Id, gameTypeValue);
                
                foreach (var user in users)
                {
                    var mscore = AlgorithmHelper.CalculateMatchScore(user, matcher);
                    matchPool.Add(mscore);

                    user.Games = _gameService.ListByUserId(user.Id);
                }
                
                // sort matchPool by Score descending
                matchPool = matchPool.OrderByDescending(m => m.Score).ToList();
                return matchPool;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserService.GetUsersWithMatchingGameType: Exception: {ex}");
                return [];
            }
        }

        public User? Update(User user)
        {
            return _userRepository.Update(user);
        }
    }
}
