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
    public class UserRepository : IUserRepository
    {
        private readonly List<User> clientList;

        public UserRepository()
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
            var admin = new User(3, "A.J. Kwak", "user3@mail.com", "sxnIcZdYt8wC8MYWcQVQjQ==.FKd5Z/jwxPv3a63lX+uvQ0+P7EuNYZybvkmdhbnkIHA=") { Role = Role.Admin };
            clientList = new List<User>
            {
                new User(1, "M.J. Curie", "user1@mail.com", "IunRhDKa+fWo8+4/Qfj7Pg==.kDxZnUQHCZun6gLIE6d9oeULLRIuRmxmH2QKJv2IM08="),
                new User(2, "H.H. Hermans", "user2@mail.com", "dOk+X+wt+MA9uIniRGKDFg==.QLvy72hdG8nWj1FyL75KoKeu4DUgu5B/HAHqTD2UFLU="),
                admin
            };
        }

        // Get client by email address (case-insensitive)
        public User? Get(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            var found = clientList.FirstOrDefault(c => c.EmailAddress.Equals(email, StringComparison.OrdinalIgnoreCase));
            return found;
        }

        // Get client by ID
        public User? Get(int id)
        {
            if (id <= 0) return null;
            var found = clientList.FirstOrDefault(c => c.Id == id);
            return found;
        }

        // Get all clients
        public List<User> GetAll() => new(clientList);

        // Insert a client into the DB and return created client with assigned id
        public User? Add(User client)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return null;
                }

                const string sql = @"INSERT INTO `users` (`email`, `password`, `name`, `birth_date`, `region`, `bio`, `profile_picture`, `role`, `is_active`)
                                     VALUES (@email, @password, @name, @birth_date, @region, @bio, @profile_picture, @role, @is_active);";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);

                cmd.Parameters.AddWithValue("@email", client.EmailAddress ?? string.Empty);
                cmd.Parameters.AddWithValue("@password", client.Password ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", client.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@birth_date", client.BirthDate.HasValue ? (object)client.BirthDate.Value.Date : DBNull.Value);
                cmd.Parameters.AddWithValue("@region", string.IsNullOrWhiteSpace(client.Region) ? DBNull.Value : (object)client.Region);
                cmd.Parameters.AddWithValue("@bio", client.Bio ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@profile_picture", client.ProfilePicture ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@role", (int)client.Role);
                cmd.Parameters.AddWithValue("@is_active", client.IsActive);

                cmd.ExecuteNonQuery();
                var newId = (int)cmd.LastInsertedId;

                var created = new User(newId, client.Name, client.EmailAddress, client.Password)
                {
                    BirthDate = client.BirthDate,
                    Region = client.Region,
                    Bio = client.Bio,
                    ProfilePicture = client.ProfilePicture,
                    IsActive = client.IsActive,
                    Role = client.Role
                };

                clientList.Add(created);
                return created;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Load clients from the database
        private List<User>? LoadFromDb()
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
                var result = new List<User>();

                // Open DB connection
                using var conn = new MySqlConnection(connectionString);
                try
                {
                    conn.Open();
                }
                catch (Exception exOpen)
                {
                    return null;
                }

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

                    var client = new User(id, name, email, password, birthDate, region, bio, profilePicture, isActive)
                    {
                        Role = Enum.IsDefined(typeof(Role), (ushort)roleInt) ? (Role)roleInt : Role.None
                    };

                    result.Add(client);
                }

                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}