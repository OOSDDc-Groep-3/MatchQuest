using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        public User? Get(string email);
        public User? Get(int id);
        public List<User> GetAll();
        public User? Add(User client);
        public User? Update(User client);
        List<User> GetUsersWithMatchingGameType(int userId);
    }
}
