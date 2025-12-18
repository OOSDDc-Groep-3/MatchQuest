using System.Data;
using System.Data.Common;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using MatchQuest.Core.Data.Helpers;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace MatchQuest.Core.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        // Get client by email address (case-insensitive)
        public User? Get(string email)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                var sql = @"Select * from users where email = @email;";
                
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
                Debug.WriteLine(ex);
                return null;
            }
        }

        // Get client by ID
        public User? Get(int id)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                var sql = @"Select * from users where user_id = @id;";
                
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
                return null;
            }
        }

        // Get all clients
        public List<User> GetAll()
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                var qry = "select * from users";
                
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(qry, conn);
                using var reader = cmd.ExecuteReader();
                var clients = new List<User>();
                while (reader.Read())
                {
                    var client = MapUser(reader);
                    if (client != null) clients.Add(client);
                }
                
                return clients;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        // Insert a client into the DB and return created client with assigned id
        public User? Add(User client)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"INSERT INTO `users` (`email`, `password`, `name`, `birth_date`, `region`, `bio`, `profile_picture`, `role`, `is_active`)
                                     VALUES (@email, @password, @name, @birth_date, @region, @bio, @profile_picture, @role, @is_active);";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                
                // tranform Birthday DateOnly to Datetime
                DateTime? birthDateTime = client.BirthDate.HasValue
                    ? new DateTime(client.BirthDate.Value.Year, client.BirthDate.Value.Month, client.BirthDate.Value.Day)
                    : null;
                
                cmd.Parameters.AddWithValue("@email", client.Email ?? string.Empty);
                cmd.Parameters.AddWithValue("@password", client.Password ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", client.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@birth_date", birthDateTime.HasValue ? (object)birthDateTime.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@region", string.IsNullOrWhiteSpace(client.Region) ? DBNull.Value : (object)client.Region);
                cmd.Parameters.AddWithValue("@bio", client.Biography ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@profile_picture", client.ProfilePicture ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@role", (int)client.Role);
                cmd.Parameters.AddWithValue("@is_active", client.IsActive);

                cmd.ExecuteNonQuery();
                var newId = (int)cmd.LastInsertedId;

                return Get(newId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        
        
        public User? Update(User client)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"UPDATE `users` 
                               SET `email` = @email,
                                   `password` = @password,
                                   `name` = @name,
                                   `birth_date` = @birth_date,
                                   `region` = @region,
                                   `bio` = @bio,
                                   `profile_picture` = @profile_picture,
                                   `role` = @role,
                                   `is_active` = @is_active
                             WHERE `user_id` = @user_id;";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                
                // transform DateOnly to DateTime for DB storage
                var birthDateTime = client.BirthDate.HasValue
                    ? new DateTime(client.BirthDate.Value.Year, client.BirthDate.Value.Month, client.BirthDate.Value.Day)
                    : (DateTime?)null;
                
                cmd.Parameters.AddWithValue("@user_id", client.Id);
                cmd.Parameters.AddWithValue("@email", client.Email ?? string.Empty);
                cmd.Parameters.AddWithValue("@password", client.Password ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", client.Name ?? string.Empty);
                cmd.Parameters.AddWithValue("@birth_date", birthDateTime.HasValue ? birthDateTime.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@region", string.IsNullOrWhiteSpace(client.Region) ? DBNull.Value : (object)client.Region);
                cmd.Parameters.AddWithValue("@bio", client.Biography ?? string.Empty);
                cmd.Parameters.AddWithValue("@profile_picture", client.ProfilePicture ?? string.Empty);
                
                cmd.Parameters.AddWithValue("@role", (int)client.Role);
                cmd.Parameters.AddWithValue("@is_active", client.IsActive);

                cmd.ExecuteNonQuery();

                return Get(client.Id);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<User> GetUsersWithMatchingGameType(int userId, int gameTypeId)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"
SELECT
    u.*,
    -- CHECK if the user has liked the current user
    CASE
        WHEN EXISTS (
            SELECT 1
            FROM reactions r
            WHERE r.user_id = u.user_id
              AND r.target_user_id = @CurrentUserId
              AND r.is_like = 1
        ) THEN 1
        ELSE 0
        END AS has_liked_you
FROM users u
WHERE u.user_id != @CurrentUserId

  -- exclude existing matches
  AND NOT EXISTS (
    SELECT 1
    FROM matches m
    WHERE (m.user1_id = @CurrentUserId AND m.user2_id = u.user_id)
       OR (m.user1_id = u.user_id AND m.user2_id = @CurrentUserId)
)

  -- exclude users the current user ALREADY liked
  AND NOT EXISTS (
    SELECT 1
    FROM reactions l_sent
    WHERE l_sent.user_id = @CurrentUserId
      AND l_sent.target_user_id = u.user_id
      AND l_sent.is_like = 1
)

  -- exclude users the current user ALREADY disliked
  AND NOT EXISTS (
    SELECT 1
    FROM reactions d_sent
    WHERE d_sent.user_id = @CurrentUserId
      AND d_sent.target_user_id = u.user_id
      AND d_sent.is_like = 0
)";

                if (gameTypeId > 0)
                {
                    sql += @"
-- Require at least one game of a specific type
    AND EXISTS (
      SELECT 1
      FROM user_games ug_candidate
      JOIN games g_candidate ON ug_candidate.game_id = g_candidate.game_id
      WHERE ug_candidate.user_id = u.user_id
      AND g_candidate.type = @GameTypeId
    )";
                }
                else
                {
                    sql += @"
  -- require at least one game type in common
  AND EXISTS (
    SELECT 1
    FROM user_games ug_candidate
    JOIN games g_candidate ON ug_candidate.game_id = g_candidate.game_id
    WHERE ug_candidate.user_id = u.user_id
    AND g_candidate.type IN (
        SELECT g_me.type
        FROM user_games ug_me
        JOIN games g_me ON ug_me.game_id = g_me.game_id
        WHERE ug_me.user_id = @CurrentUserId
      )
)";
                }

                sql += @"
LIMIT 50;";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@CurrentUserId", userId);
                if (gameTypeId > 0)
                {
                    cmd.Parameters.AddWithValue("@GameTypeId", gameTypeId);
                }

                var users = new List<User>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var user = MapUser(reader);
                    if (user != null)
                    {
                        users.Add(user);
                    }
                }
                return users;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserRepository.GetUsersWithMatchingGameType: Exception: {ex}");
                return new List<User>();
            }
        }

        public User? MapUser(DbDataReader reader)
        {
            var id = reader.GetInt32("user_id");
            var name = reader.GetString("name");
            var emailAddr = reader.GetString("email");
            var password = reader.GetString("password");
            var roleInt = reader.GetInt32("role");

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

            var isActive = reader.GetBoolean("is_active");
            
            DateOnly? birthDate = birthDateTime.HasValue
                ? DateOnly.FromDateTime(birthDateTime.Value)
                : null;
            
            var createdAt = reader.GetDateTime("created_at");
            DateTime? updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime("updated_at");

            var client = new User(id, name, emailAddr, password, birthDate, region, bio, profilePicture, isActive, createdAt, updatedAt)
            {
                Role = Enum.IsDefined(typeof(Role), (ushort)roleInt) ? (Role)roleInt : Role.None
            };

            return client;
        }
    }
}