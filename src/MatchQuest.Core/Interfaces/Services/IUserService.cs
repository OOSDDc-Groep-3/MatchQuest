using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services
{
    public interface IUserService
    {
        public User? Get(string email);
        public User? Get(int id);
        public List<User> GetAll();
        public User? Create(User client);
    }
}
