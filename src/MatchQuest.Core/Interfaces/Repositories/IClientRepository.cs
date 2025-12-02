using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories
{
    public interface IClientRepository
    {
        public Client? Get(string email);
        public Client? Get(int id);
        public List<Client> GetAll();
        public Client? Add(Client client);
    }
}
