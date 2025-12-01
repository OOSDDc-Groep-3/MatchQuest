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
            // Try to load clients from database; fall back to seeded list only if an error occurred (LoadFromDb returned null).
            var dbClients = LoadFromDb();
            if (dbClients != null)
            {
                // Use DB result (even if empty). This avoids silently using fallback when DB table is simply empty.
                clientList = dbClients;
                if (clientList.Count == 0)
                    Debug.WriteLine("ClientRepository: Users table returned 0 rows.");
                return;
            }

            Debug.WriteLine("ClientRepository: Falling back to seeded clients.");

            var admin = new Client(3, "A.J. Kwak", "user3@mail.com", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=");
            admin.Role = Role.Admin;

            clientList = new List<Client>
            {
                new Client(1, "M.J. Curie", "user1@mail.com", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08="),
                new Client(2, "H.H. Hermans", "user2@mail.com", "dOk+X+wt+MA9uIniRGKDFg==.QLvy72hdG8nWj1FyL75KoKeu4DUgu5B/HAHqTD2UFLU="),
                admin
            };
        }

        public Client? Get(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            return clientList.FirstOrDefault(c => c.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        public Client? Get(int id)
        {
            if (id <= 0) return null;
            return clientList.FirstOrDefault(c => c.Id == id);
        }

        public List<Client> GetAll()
        {
            return new List<Client>(clientList);
        }

        private List<Client>? LoadFromDb()
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                Debug.WriteLine($"ClientRepository: DefaultConnection found? {!string.IsNullOrWhiteSpace(connectionString)}");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    Debug.WriteLine("ClientRepository: DefaultConnection not found in configuration.");
                    return null;
                }

                const string sql = @"
SELECT `User_Id`, `Name`, `Email`, `Password`, `Role`, `birth_date`, `region`, `bio`, `profile_picture`, `is_active`
FROM `Users`
ORDER BY `User_Id`;";

                var result = new List<Client>();

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

                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    // Helper locals to try alternative column names
                    int? GetInt(params string[] names)
                    {
                        foreach (var n in names)
                        {
                            try
                            {
                                var ord = reader.GetOrdinal(n);
                                if (!reader.IsDBNull(ord)) return reader.GetInt32(ord);
                            }
                            catch (IndexOutOfRangeException) { /* try next */ }
                        }
                        return null;
                    }

                    string? GetString(params string[] names)
                    {
                        foreach (var n in names)
                        {
                            try
                            {
                                var ord = reader.GetOrdinal(n);
                                if (!reader.IsDBNull(ord)) return reader.GetString(ord);
                            }
                            catch (IndexOutOfRangeException) { /* try next */ }
                        }
                        return null;
                    }

                    DateTime? GetDate(params string[] names)
                    {
                        foreach (var n in names)
                        {
                            try
                            {
                                var ord = reader.GetOrdinal(n);
                                if (!reader.IsDBNull(ord)) return reader.GetDateTime(ord);
                            }
                            catch (IndexOutOfRangeException) { /* try next */ }
                        }
                        return null;
                    }

                    bool? GetBool(params string[] names)
                    {
                        foreach (var n in names)
                        {
                            try
                            {
                                var ord = reader.GetOrdinal(n);
                                if (!reader.IsDBNull(ord)) return reader.GetBoolean(ord);
                            }
                            catch (IndexOutOfRangeException) { /* try next */ }
                        }
                        return null;
                    }

                    var id = GetInt("User_Id", "Id", "user_id") ?? 0;
                    var name = GetString("Name") ?? string.Empty;
                    // DB sometimes has column Email or EmailAddress — handle both
                    var email = GetString("Email", "EmailAddress") ?? string.Empty;
                    var password = GetString("Password") ?? string.Empty;

                    var birthDate = GetDate("birth_date");
                    var region = GetString("region");
                    var bio = GetString("bio");
                    var profilePicture = GetString("profile_picture");
                    var isActive = GetBool("is_active") ?? true;

                    var roleInt = GetInt("Role") ?? 0;

                    var client = new Client(id, name, email, password, birthDate, region, bio, profilePicture, isActive);

                    if (Enum.IsDefined(typeof(Role), (ushort)roleInt))
                    {
                        client.Role = (Role)roleInt;
                    }
                    else
                    {
                        client.Role = Role.None;
                    }

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