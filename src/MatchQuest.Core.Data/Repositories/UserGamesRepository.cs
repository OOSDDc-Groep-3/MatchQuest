using System.Diagnostics;
using MatchQuest.Core.Data.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MySql.Data.MySqlClient;

namespace MatchQuest.Core.Data.Repositories
{
    public class UserGameRepository : IUserGameRepository
    {
        public bool AddUserGame(int userId, int gameId)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                var sql = @"INSERT INTO user_games (user_id, game_id) VALUES (@userId, @gameId);";
                
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@gameId", gameId);
                
                // execute the command and check the number of affected rows
                var rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserGameRepository.AddUserGame: {ex}");
                return false;
            }
        }

        public bool RemoveUserGame(int userId, int gameId)
        {
            try
            {
                var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
                var sql = @"DELETE FROM user_games WHERE user_id = @userId AND game_id = @gameId;";
                
                using var conn = new MySqlConnection(connectionString);
                conn.Open();
                
                using var cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@gameId", gameId);

                var rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"UserGameRepository.RemoveUserGame: {ex}");
                return false;
            }
        }
    }
}
