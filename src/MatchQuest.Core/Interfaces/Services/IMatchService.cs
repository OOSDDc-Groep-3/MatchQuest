using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services
{
    public interface IMatchService
    {
        public User? Get(string email);
        public User? Get(int id);
        public List<User> GetAll();
        public List<User> GetAll(int userId);
        public User? Create(User client);
    }
}
