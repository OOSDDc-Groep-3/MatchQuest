namespace MatchQuest.Core.Interfaces.Repositories;

public interface IUserGameRepository
{
    bool AddUserGame(int userId, int gameId);
    bool RemoveUserGame(int userId, int gameId);
}