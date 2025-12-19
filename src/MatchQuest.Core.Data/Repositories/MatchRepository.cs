using System.Diagnostics;
using MatchQuest.Core.Data.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using MySql.Data.MySqlClient;

namespace MatchQuest.Core.Data.Repositories
{
    public class MatchRepository : IMatchRepository
    {
        private readonly IUserRepository _userRepository;

        public MatchRepository(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public List<User> GetAllMatchesFromUserId(int userId)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"
SELECT DISTINCT u.*
FROM users u
INNER JOIN matches m
    ON (
        (m.user1_id = @userId AND u.user_id = m.user2_id)
        OR (m.user2_id = @userId AND u.user_id = m.user1_id)
    );";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                using var reader = cmd.ExecuteReader();

                var users = new List<User>();
                while (reader.Read())
                {
                    var user = _userRepository.MapUser(reader);
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

        public Match? GetByUserIds(int userId, int userId2)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"
SELECT *
FROM Matches
WHERE 
    (user1_id = @userId AND user2_id = @userId2)
 OR (user1_id = @userId2 AND user2_id = @userId);";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@userId2", userId2);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    return MapMatch(reader);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"GetByUserIds: {ex}");
                return null;
            }
        }

        public Match? CreateMatch(int userId1, int userId2)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                var sql = @"
INSERT INTO matches(user1_id, user2_id)
VALUES (@User1Id, @User2Id)";

                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@User1Id", userId1);
                cmd.Parameters.AddWithValue("@User2Id", userId2);

                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected > 0)
                {
                    return GetByUserIds(userId1, userId2);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"CreateIfNotExists: {ex}");
                return null;
            }
        }

        private Match? MapMatch(MySqlDataReader reader)
        {
            var id = reader.GetInt32("match_id");
            var user1Id = reader.GetInt32("user1_id");
            var user2Id = reader.GetInt32("user2_id");
            var createdAt = reader.GetDateTime("created_at");
            DateTime? updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at"))
                ? null
                : reader.GetDateTime("updated_at");

            return new Match(id, user1Id, user2Id, createdAt, updatedAt);
        }
    }
}