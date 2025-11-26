
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services
{
    public interface IAuthService
    {
        Client? Login(string email, string password);
    }
}
