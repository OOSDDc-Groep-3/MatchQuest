using System;
using System.Collections.Generic;
using System.Diagnostics;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services
{
    public class MatchService : IMatchService
    {
        private readonly IMatchRepository _matchRepository;
        public MatchService(IMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        public User? Get(string email)
        {
            var result = _matchRepository.Get(email);
            return result;
        }

        public User? Get(int id)
        {
            var result = _matchRepository.Get(id);
            return result;
        }

        public List<User> GetAll()
        {
            var clients = _matchRepository.GetAll();
            return clients;
        }

        public List<User> GetAll(int userId)
        {
            var matches = _matchRepository.GetAll(userId);
            return matches;
        }

        public User? Create(User client)
        {
            try
            {
                var created = _matchRepository.Add(client);
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
