using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services
{
    public interface IUserService
    {
        User? Get(string email);
        User? Get(int id);
        List<User> GetAll();
        User? Create(User client);
        List<MatchingScore> GetUserMatchPool(User user);
        User? Update(User user);
    }
}
