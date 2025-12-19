using System.Collections.Generic;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories;

public interface IGameRepository
{
    List<Game> GetAll();
    List<Game> ListByUserId(int userId);
    Game? Get(int gameId);
}