using System;
using System.Collections.Generic;
using System.Diagnostics;
using MatchQuest.Core.Data.Helpers;
using MatchQuest.Core.Models;
using MySql.Data.MySqlClient;

namespace MatchQuest.Core.Data.Repositories
{
    public class UserGameRepository
    {
        public UserGameRepository() { }

       
        public void AddUserGame(int userId, int gameId)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                using var cmd = new MySqlCommand(@"
INSERT INTO user_games (user_id, game_id, created_at, updated_at)
VALUES (@userId, @gameId, @createdAt, @updatedAt);", conn);

                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserGameRepository.AddUserGame: {ex}");
            }
        }

        public void RemoveUserGame(int userId, int gameId)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                using var cmd = new MySqlCommand(@"
DELETE FROM user_games 
WHERE user_id = @userId AND game_id = @gameId;", conn);

                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@gameId", gameId);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserGameRepository.RemoveUserGame: {ex}");
                throw;
            }
        }

        public List<int> GetUserGameIds(int userId)
        {
            var result = new List<int>();
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

                using var conn = new MySqlConnection(connectionString);
                conn.Open();

                using var cmd = new MySqlCommand(@"
SELECT game_id FROM user_games WHERE user_id = @userId;", conn);

                cmd.Parameters.AddWithValue("@userId", userId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    result.Add(reader.GetInt32("game_id"));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserGameRepository.GetUserGameIds: {ex}");
            }

            return result;
        }
        
        public List<Game> GetGamesForUser(int userId)
        {
            var games = new List<Game>();
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            using var cmd = new MySqlCommand(@"
SELECT g.game_id, g.name,g.image
FROM user_games ug
INNER JOIN games g ON ug.game_id = g.game_id
WHERE ug.user_id = @userId;", conn);

            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                games.Add(new Game
                {
                    Id = reader.GetInt32("game_id"),
                    Name = reader.GetString("name"),
                    Image = reader.IsDBNull(reader.GetOrdinal("image")) 
                        ? "carlala.png" // fallback image
                        : reader.GetString("image")
                });
            }

            return games;
        }
    }
}
