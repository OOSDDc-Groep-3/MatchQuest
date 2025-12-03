using System;
using System.Collections.Generic;
using System.Diagnostics;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public User? Get(string email)
        {
            var result = _userRepository.Get(email);
            return result;
        }

        public User? Get(int id)
        {
            var result = _userRepository.Get(id);
            return result;
        }

        public List<User> GetAll()
        {
            var clients = _userRepository.GetAll();
            return clients;
        }

        public User? Create(User client)
        {
            try
            {
                var created = _userRepository.Add(client);
                return created;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ClientService.Create: Exception: {ex}");
                return null;
            }
        }
    }
}
