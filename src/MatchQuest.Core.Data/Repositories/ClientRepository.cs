using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using MatchQuest.Core.Data.Helpers;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MatchQuest.Core.Data.Repositories
{
    public class ClientRepository : IClientRepository
    {
        private readonly List<Client> clientList;

        public ClientRepository()
        {
            // Load from DB; fall back only when an error occurs (LoadFromDb returns null).
            var dbClients = LoadFromDb();

            // If dbClients is null, an error occurred; fall back to seeded clients.
            if (dbClients != null)
            {
                clientList = dbClients;
                if (clientList.Count == 0)
                return;
            }

            // Seeded clients as a fallback
            var admin = new Client(3, "A.J. Kwak", "user3@mail.com", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=") { Role = Role.Admin };
            clientList = new List<Client>
            {
                new Client(1, "M.J. Curie", "user1@mail.com", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08="),
                new Client(2, "H.H. Hermans", "user2@mail.com", "dOk+X+wt+MA9uIniRGKDFg==.QLvy72hdG8nWj1FyL75KoKeu4DUgu5B/HAHqTD2UFLU="),
                admin
            };
        }

        // Get client by email address (case-insensitive)
        public Client? Get(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return clientList.FirstOrDefault(c => c.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        // Get client by ID
        public Client? Get(int id)
        {
            if (id <= 0) return null;
            return clientList.FirstOrDefault(c => c.Id == id);
        }

        // Get all clients
        public List<Client> GetAll() => new(clientList);

        // Load clients from the database
        private List<Client>? LoadFromDb()
        {
            try
            {
                // Get connection string
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                // Check if connection string is valid else return null
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return null;
                }

                // SQL query to select all clients
                const string sql = "SELECT `user_id`, `name`, `email`, `password`, `role`, `birth_date`, `region`, `bio`, `profile_picture`, `is_active` FROM `users` ORDER BY `user_id`;";

                // Result list
                var result = new List<Client>();

                // Open DB connection
                using var conn = new MySqlConnection(connectionString);
                try
                {
                    conn.Open();
                }
                catch (Exception exOpen)
                {
                    Debug.WriteLine($"ClientRepository: Failed to open DB connection: {exOpen}");
                    return null;
                }

                // Execute SQL command
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var idOrd = reader.GetOrdinal("user_id");
                    var nameOrd = reader.GetOrdinal("name");
                    var emailOrd = reader.GetOrdinal("email");
                    var passwordOrd = reader.GetOrdinal("password");
                    var roleOrd = reader.GetOrdinal("role");
                    var birthOrd = reader.GetOrdinal("birth_date");
                    var regionOrd = reader.GetOrdinal("region");
                    var bioOrd = reader.GetOrdinal("bio");
                    var picOrd = reader.GetOrdinal("profile_picture");
                    var activeOrd = reader.GetOrdinal("is_active");

                    var id = reader.IsDBNull(idOrd) ? 0 : reader.GetInt32(idOrd);
                    var name = reader.IsDBNull(nameOrd) ? string.Empty : reader.GetString(nameOrd);
                    var email = reader.IsDBNull(emailOrd) ? string.Empty : reader.GetString(emailOrd);
                    var password = reader.IsDBNull(passwordOrd) ? string.Empty : reader.GetString(passwordOrd);
                    var roleInt = reader.IsDBNull(roleOrd) ? 0 : reader.GetInt32(roleOrd);
                    DateTime? birthDate = reader.IsDBNull(birthOrd) ? null : reader.GetDateTime(birthOrd);
                    var region = reader.IsDBNull(regionOrd) ? null : reader.GetString(regionOrd);
                    var bio = reader.IsDBNull(bioOrd) ? null : reader.GetString(bioOrd);
                    var profilePicture = reader.IsDBNull(picOrd) ? null : reader.GetString(picOrd);
                    var isActive = reader.IsDBNull(activeOrd) ? true : reader.GetBoolean(activeOrd);

                    var client = new Client(id, name, email, password, birthDate, region, bio, profilePicture, isActive)
                    {
                        Role = Enum.IsDefined(typeof(Role), (ushort)roleInt) ? (Role)roleInt : Role.None
                    };

                    result.Add(client);
                }

                Debug.WriteLine($"ClientRepository: Loaded {result.Count} clients from DB.");
                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ClientRepository: Exception while loading clients: {ex}");
                return null;
            }
        }
    }
}