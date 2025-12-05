
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services
{
    public interface IAuthService
    {
        User? Login(string email, string password);
    }
}
