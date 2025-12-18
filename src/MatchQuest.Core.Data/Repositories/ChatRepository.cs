using System;
using System.Collections.Generic;
using MatchQuest.Core.Models;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Data.Helpers;
using MySql.Data.MySqlClient;
using System.Diagnostics;

namespace MatchQuest.Core.Data.Repositories;

public class ChatRepository : IChatRepository
{
    // Get existing chat by match ID, or create a new one if it doesn't exist
    public int GetOrCreateChatByMatchId(int matchId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT chat_id FROM chats WHERE match_id = @matchId LIMIT 1;";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matchId", matchId);

            var found = cmd.ExecuteScalar();
            if (found != null && found != DBNull.Value)
            {
                return Convert.ToInt32(found);
            }

            // Create new chat if not found
            sql = @"INSERT INTO chats (match_id, created_at) VALUES (@matchId, @createdAt);";
            using var insertCmd = new MySqlCommand(sql, conn);
            insertCmd.Parameters.AddWithValue("@matchId", matchId);
            insertCmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
            insertCmd.ExecuteNonQuery();

            var newId = (int)insertCmd.LastInsertedId;
            return newId;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.GetOrCreateChatByMatchId: Exception: {ex}");
            return 0;
        }
    }

    // Get all messages for a specific chat, ordered by creation time
    public List<Message> GetMessagesByChatId(int chatId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT message_id, chat_id, sender_id, message_text, created_at, updated_at
                        FROM messages
                        WHERE chat_id = @chatId
                        ORDER BY created_at ASC;";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@chatId", chatId);

            var result = new List<Message>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var m = new Message
                {
                    Id = reader.GetInt32("message_id"),
                    ChatId = reader.GetInt32("chat_id"),
                    SenderId = reader.GetInt32("sender_id"),
                    MessageText = reader.GetString("message_text"),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("created_at")) ? DateTime.MinValue : reader.GetDateTime("created_at"),
                };
                result.Add(m);
            }

            return result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.GetMessagesByChatId: Exception: {ex}");
            return new List<Message>();
        }
    }

    // Check if a user exists in the database
    public bool UserExists(int userId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT 1 FROM users WHERE user_id = @id LIMIT 1;";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            var found = cmd.ExecuteScalar();
            return found != null && found != DBNull.Value;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.UserExists: Exception: {ex}");
            return false;
        }
    }

    // Check if a chat exists in the database
    public bool ChatExists(int chatId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT 1 FROM chats WHERE chat_id = @id LIMIT 1;";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", chatId);

            var found = cmd.ExecuteScalar();
            return found != null && found != DBNull.Value;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.ChatExists: Exception: {ex}");
            return false;
        }
    }

    // Create a user if they don't exist (for demo/seeded data purposes)
    public void CreateUserIfNotExists(int userId, string name, string email)
    {
        try
        {
            if (UserExists(userId)) return;

            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"INSERT INTO users (user_id, email, password, name, birth_date, role, is_active, created_at)
                        VALUES (@id, @email, @password, @name, @birthDate, @role, 1, @createdAt);";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);
            cmd.Parameters.AddWithValue("@email", email ?? $"user{userId}@example.com");
            cmd.Parameters.AddWithValue("@password", "changeme"); // demo: replace with proper hashing in production
            cmd.Parameters.AddWithValue("@name", name ?? $"User{userId}");
            cmd.Parameters.AddWithValue("@birthDate", new DateTime(1990, 1, 1));
            cmd.Parameters.AddWithValue("@role", "User");
            cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
            cmd.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.CreateUserIfNotExists: Exception: {ex}");
        }
    }

    // Insert a new message into the database and return the generated message ID
    public int InsertMessage(Message message)
    {
        try
        {
            // Validate sender exists to satisfy FK constraint
            if (!UserExists(message.SenderId))
                throw new InvalidOperationException($"Sender id {message.SenderId} does not exist in users table. Message not inserted.");

            // Validate chat exists
            if (!ChatExists(message.ChatId))
                throw new InvalidOperationException($"Chat id {message.ChatId} does not exist. Create or retrieve the chat before inserting messages.");

            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"INSERT INTO messages (chat_id, sender_id, message_text, created_at)
                        VALUES (@chatId, @senderId, @text, @createdAt);";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@chatId", message.ChatId);
            cmd.Parameters.AddWithValue("@senderId", message.SenderId);
            cmd.Parameters.AddWithValue("@text", message.MessageText);
            cmd.Parameters.AddWithValue("@createdAt", message.CreatedAt == default ? DateTime.UtcNow : message.CreatedAt);

            cmd.ExecuteNonQuery();
            var newId = (int)cmd.LastInsertedId;
            return newId;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.InsertMessage: Exception: {ex}");
            return 0;
        }
    }

    // Get the first match ID for a specific user (oldest match)
    public int GetMatchIdForUser(int userId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT match_id
                        FROM matches
                        WHERE user1_id = @id OR user2_id = @id
                        ORDER BY created_at ASC
                        LIMIT 1;";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", userId);

            var found = cmd.ExecuteScalar();
            return (found != null && found != DBNull.Value) ? Convert.ToInt32(found) : 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.GetMatchIdForUser: Exception: {ex}");
            return 0;
        }
    }

    // Get the other user's ID in a match (the user who is not the current user)
    public int GetOtherUserIdForMatch(int matchId, int currentUserId)
    {
        try
        {
            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT user1_id, user2_id
                        FROM matches
                        WHERE match_id = @matchId
                        LIMIT 1;";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@matchId", matchId);

            using var reader = cmd.ExecuteReader();
            if (!reader.Read())
            {
                return 0;
            }

            var user1 = reader.IsDBNull(reader.GetOrdinal("user1_id")) ? 0 : reader.GetInt32("user1_id");
            var user2 = reader.IsDBNull(reader.GetOrdinal("user2_id")) ? 0 : reader.GetInt32("user2_id");

            if (currentUserId == 0) return 0;
            if (user1 == currentUserId) return user2;
            if (user2 == currentUserId) return user1;
            return 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.GetOtherUserIdForMatch: Exception: {ex}");
            return 0;
        }
    }

    // Find the match ID between two users (regardless of who is user1 or user2)
    public int GetMatchIdBetween(int userAId, int userBId)
    {
        try
        {
            if (userAId == 0 || userBId == 0) return 0;

            var connectionString = ConnectionHelper.ConnectionStringValue("DefaultConnection");
            var sql = @"SELECT match_id
                        FROM matches
                        WHERE (user1_id = @user1 AND user2_id = @user2) OR (user1_id = @user2 AND user2_id = @user1)
                        LIMIT 1;";

            using var conn = new MySqlConnection(connectionString);
            conn.Open();
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@user1", userAId);
            cmd.Parameters.AddWithValue("@user2", userBId);

            var found = cmd.ExecuteScalar();
            return (found != null && found != DBNull.Value) ? Convert.ToInt32(found) : 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ChatRepository.GetMatchIdBetween: Exception: {ex}");
            return 0;
        }
    }
}