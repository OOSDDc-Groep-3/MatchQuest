using MatchQuest.Core.Helpers;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _clientService;
        public AuthService(IUserService clientService)
        {
            _clientService = clientService;
        }
        public User? Login(string email, string password)
        {
            User? client = _clientService.Get(email);
            if (client == null) return null;
            if (PasswordHelper.VerifyPassword(password, client.Password)) return client;
            return null;
        }
    }
}
