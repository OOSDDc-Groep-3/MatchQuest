    using System.Collections.Generic;
    using MatchQuest.Core.Data.Helpers;
    using MatchQuest.Core.Interfaces.Repositories;
    using MatchQuest.Core.Models;
    using MySql.Data.MySqlClient;

    namespace MatchQuest.Core.Data.Repositories;

    public class GameRepository : IGameRepository
    {
        public Game? Get(int gameId)
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

            var sql = @"
SELECT *
FROM games
WHERE game_id = @GameId;";
            
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@GameId", gameId);
            
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return MapGame(reader);
            }
            
            return null;
        }
        
        public List<Game> GetAll()
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

            var sql = @"
SELECT *
FROM games
ORDER BY name ASC;";
            
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);

            var games = new List<Game>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var game = MapGame(reader);
                if (game != null)
                {
                    games.Add(game);
                }
            }
            
            return games;
        }

        public List<Game> ListByUserId(int userId)
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");

            var sql = @"
SELECT g.* FROM games g
INNER JOIN user_games ug
    ON g.game_id = ug.game_id
WHERE ug.user_id = @UserId;";
            
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);

            var games = new List<Game>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var game = MapGame(reader);
                if (game != null)
                {
                    games.Add(game);
                }
            }
            
            return games;
        }
        
        public Game? MapGame(MySqlDataReader reader)
        {
            try
            {
                var id = reader.GetInt32("game_id");
                var name = reader.GetString("name");
                var typeRaw = reader.GetInt32("type");
                var type = GameTypeFromInt(typeRaw);
                var approved = reader.GetBoolean("approved");
                var image = !reader.IsDBNull(reader.GetOrdinal("image")) ? reader.GetString("image") : string.Empty;
                var createdAt = reader.GetDateTime("created_at");
                DateTime? updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at"))
                    ? null
                    : reader.GetDateTime("updated_at");

                return new Game(id, name, type, approved, image, createdAt, updatedAt);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error mapping Game: {ex.Message}");
                return null;
            }
        }
        
        private GameType GameTypeFromInt(int value)
        {
            if (Enum.IsDefined(typeof(GameType), value))
            {
                return (GameType)value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(value), $"No GameType defined for value {value}");
            }
        }
    }