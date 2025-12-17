using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services;

public class GameService : IGameService
{
    private IGameRepository _gameRepository;
    private IUserGameRepository _userGameRepository;
    
    public GameService(IGameRepository gameRepository, IUserGameRepository userGameRepository)
    {
        _gameRepository = gameRepository;
        _userGameRepository = userGameRepository;
    }
    
    public List<Game> GetAll()
    {
        return _gameRepository.GetAll();
    }
    
    public List<Game> ListByUserId(int userId)
    {
        return _gameRepository.ListByUserId(userId);
    }

    public Game Get(int gameId)
    {
        return _gameRepository.Get(gameId);
    }

    public Game? AddGameToUser(int gameId, int userId)
    {
        if (!_userGameRepository.AddUserGame(userId, gameId))
        {
            return null;
        }
        
        return Get(gameId);
    }

    public bool RemoveGameFromUser(int gameId, int userId)
    {
        return _userGameRepository.RemoveUserGame(userId, gameId);
    }
}