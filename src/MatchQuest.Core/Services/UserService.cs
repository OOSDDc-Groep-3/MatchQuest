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
        private readonly IUserRepository _clientRepository;
        public UserService(IUserRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public User? Get(string email)
        {
            var result = _clientRepository.Get(email);
            return result;
        }

        public User? Get(int id)
        {
            var result = _clientRepository.Get(id);
            return result;
        }

        public List<User> GetAll()
        {
            var clients = _clientRepository.GetAll();
            return clients;
        }

        public User? Create(User client)
        {
            try
            {
                var created = _clientRepository.Add(client);
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
