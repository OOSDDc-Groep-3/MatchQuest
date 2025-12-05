using System;
using System.Collections.Generic;
using System.Diagnostics;
using MatchQuest.Core.Data.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using MySql.Data.MySqlClient;

namespace MatchQuest.Core.Data.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        public MatchRepository() { }

        public User? Get(string email)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                var sql = @"SELECT * FROM users WHERE email = @email;";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@email", email);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return MapUser(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MatchRepository.Get(email): {ex}");
                return null;
            }
        }

        // Return a user by id (reuses users table behaviour)
        public User? Get(int id)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                var sql = @"SELECT * FROM users WHERE user_id = @id;";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@id", id);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    return MapUser(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MatchRepository.Get(id): {ex}");
                return null;
            }
        }

        // Return all users that appear in the matches table (deduplicated)
        public List<User> GetAll()
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"
SELECT DISTINCT u.*
FROM users u
INNER JOIN matches m
    ON u.user_id = m.user1_id OR u.user_id = m.user2_id;";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                var users = new List<User>();
                while (reader.Read())
                {
                    var user = MapUser(reader);
                    if (user != null) users.Add(user);
                }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MatchRepository.GetAll: {ex}");
                return new List<User>();
            }
        }

        // Return only matches for a specific user (the other user in each match)
        public List<User> GetAll(int userId)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"
SELECT DISTINCT u.*
FROM users u
INNER JOIN matches m
    ON (m.user1_id = @userId AND u.user_id = m.user2_id)
    OR (m.user2_id = @userId AND u.user_id = m.user1_id);";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);

                using var reader = cmd.ExecuteReader();
                var users = new List<User>();
                while (reader.Read())
                {
                    var user = MapUser(reader);
                    if (user != null) users.Add(user);
                }

                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MatchRepository.GetAll(userId): {ex}");
                return new List<User>();
            }
        }

        // Not implemented: interface requires Add(User) but matches are a relation between two users.
        // Returning null to indicate not created. Implement if you want ability to insert into matches table.
        public User? Add(User client)
        {
            Debug.WriteLine("MatchRepository.Add: Not implemented for matches.");
            return null;
        }

        // Duplicate of the UserRepository mapping logic so we can materialize User objects
        private User? MapUser(MySqlDataReader reader)
        {
            try
            {
                var id = reader.GetInt32("user_id");
                var name = reader.GetString("name");
                var emailAddr = reader.GetString("email");
                var password = reader.IsDBNull(reader.GetOrdinal("password")) ? string.Empty : reader.GetString("password");

                // role may be stored differently — attempt to read as int; fallback to None
                int roleInt = 0;
                try { roleInt = reader.IsDBNull(reader.GetOrdinal("role")) ? 0 : reader.GetInt32("role"); } catch { }

                DateTime? birthDateTime = reader.IsDBNull(reader.GetOrdinal("birth_date"))
                    ? null
                    : reader.GetDateTime("birth_date");

                var region = reader.IsDBNull(reader.GetOrdinal("region"))
                    ? null
                    : reader.GetString("region");

                var bio = reader.IsDBNull(reader.GetOrdinal("bio"))
                    ? null
                    : reader.GetString("bio");

                var profilePicture = reader.IsDBNull(reader.GetOrdinal("profile_picture"))
                    ? null
                    : reader.GetString("profile_picture");

                var isActive = reader.IsDBNull(reader.GetOrdinal("is_active")) ? true : reader.GetBoolean("is_active");

                DateOnly? birthDate = birthDateTime.HasValue
                    ? DateOnly.FromDateTime(birthDateTime.Value)
                    : null;

                var client = new User(id, name, emailAddr, password, birthDate, region, bio, profilePicture, isActive)
                {
                    Role = Enum.IsDefined(typeof(Role), (ushort)roleInt) ? (Role)roleInt : Role.None
                };

                return client;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MatchRepository.MapUser: {ex}");
                return null;
            }
        }
    }
}