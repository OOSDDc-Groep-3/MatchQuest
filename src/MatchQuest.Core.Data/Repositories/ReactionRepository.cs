using System.Diagnostics;
using MatchQuest.Core.Data.Helpers;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using MySql.Data.MySqlClient;

namespace MatchQuest.Core.Data.Repositories;

public class ReactionRepository : IReactionRepository
{
    public ReactionRepository() { }

    public Reaction? CreateReaction(int userId, int targetUserId, bool isLike)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"
INSERT INTO Reactions (user_id, target_user_id, is_like)
VALUES (@UserId, @TargetUserId, @IsLike);
";
            
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@TargetUserId", targetUserId);
            cmd.Parameters.AddWithValue("@IsLike", isLike);
        
            var rowsAffected = cmd.ExecuteNonQuery();
            if (rowsAffected > 0)
            {
                return GetReactionByUserIdAndTargetUserId(userId, targetUserId);
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error creating reaction by user ID and target user ID", ex);
            return null;
        }
    }
    
    public Reaction? GetReactionByUserIdAndTargetUserId(int userId, int targetUserId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT * FROM Reactions WHERE user_id = @UserId AND target_user_id = @TargetUserId;";
        
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@TargetUserId", targetUserId);
        
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                return MapReaction(reader);
            }

            return null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error retrieving reaction by user ID and target user ID", ex);
            return null;
        }
    }
    
    public List<Reaction> ListByUserId(int userId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"select * from reactions where user_id = @UserId";
        
            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
        
            var reactions = new List<Reaction>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var reaction = MapReaction(reader);
                if (reaction != null)
                {
                    reactions.Add(reaction);
                }
            }

            return reactions;
        }
        catch (Exception ex)
        {
            Debug.WriteLine("Error retrieving reaction by user ID and target user ID", ex);
            return null;
        }
    }

    private Reaction MapReaction(MySqlDataReader reader)
    {
        var id = reader.GetInt32("reaction_id");
        var userId = reader.GetInt32("user_id");
        var targetUserId = reader.GetInt32("target_user_id");
        var isLike = reader.GetBoolean("is_like");
        
        var createdAt = reader.GetDateTime("created_at");
        DateTime? updatedAt = reader.IsDBNull(reader.GetOrdinal("updated_at")) ? null : reader.GetDateTime("updated_at");

        return new Reaction(id, userId, targetUserId, isLike, createdAt, updatedAt);
    }
}