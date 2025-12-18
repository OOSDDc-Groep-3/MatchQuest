using System.Data;
using System.Data.Common;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        User? Get(string email);
        User? Get(int id);
        List<User> GetAll();
        User? Add(User client);
        User? Update(User client);
        List<User> GetUsersWithMatchingGameType(int userId, int gameType);
        User? MapUser(DbDataReader reader);
    }
}
