using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories;

public interface IGameRepository
{
    List<Game> GetAll();
}