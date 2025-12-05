using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories
{
    public interface IMatchRepository
    {
        public User? Get(string email);
        public User? Get(int id);
        public List<User> GetAll();
        public List<User> GetAll(int userId);
        public User? Add(User client);
    }
}
