using System;
using System.Collections.Generic;
using System.Diagnostics;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;
        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
            Debug.WriteLine("ClientService: ctor - repository injected.");
        }

        public Client? Get(string email)
        {
            Debug.WriteLine($"ClientService.Get(email): Searching for '{email}'");
            var result = _clientRepository.Get(email);
            Debug.WriteLine($"ClientService.Get(email): found? {(result == null ? "no" : "yes")}");
            return result;
        }

        public Client? Get(int id)
        {
            Debug.WriteLine($"ClientService.Get(id): Searching for id={id}");
            var result = _clientRepository.Get(id);
            Debug.WriteLine($"ClientService.Get(id): found? {(result == null ? "no" : "yes")}");
            return result;
        }

        public List<Client> GetAll()
        {
            Debug.WriteLine("ClientService.GetAll: retrieving all clients.");
            var clients = _clientRepository.GetAll();
            Debug.WriteLine($"ClientService.GetAll: count={clients?.Count}");
            return clients;
        }

        public Client? Create(Client client)
        {
            Debug.WriteLine($"ClientService.Create: Creating client email='{client?.EmailAddress}'");
            try
            {
                var created = _clientRepository.Add(client);
                Debug.WriteLine($"ClientService.Create: created? {(created == null ? "no" : "yes (id=" + created.Id + ")")}");
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
