using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services;

public interface IGameService
{
    List<Game> GetAll();
    Game Get(int gameId);
    List<Game> ListByUserId(int userId);
    Game? AddGameToUser(int gameId, int userId);
    bool RemoveGameFromUser(int gameId, int userId);
}