using System;
using System.Collections.Generic;
using MatchQuest.Core.Models;
using MySql.Data.MySqlClient;

namespace MatchQuest.Core.Data.Repositories;

public class ChatRepository : DatabaseConnection
{
    public int GetOrCreateChatByMatchId(int matchId)
    {
        OpenConnection();
        using var cmd = Connection.CreateCommand();

        cmd.CommandText = "SELECT chat_id FROM chats WHERE match_id = @matchId LIMIT 1;";
        cmd.Parameters.AddWithValue("@matchId", matchId);

        var found = cmd.ExecuteScalar();
        if (found != null && found != DBNull.Value)
        {
            CloseConnection();
            return Convert.ToInt32(found);
        }

        // create chat
        cmd.Parameters.Clear();
        cmd.CommandText = "INSERT INTO chats (match_id, created_at) VALUES (@matchId, @createdAt);";
        cmd.Parameters.AddWithValue("@matchId", matchId);
        cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT LAST_INSERT_ID();";
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        CloseConnection();
        return id;
    }

    public List<Message> GetMessagesByChatId(int chatId)
    {
        var result = new List<Message>();
        OpenConnection();
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = @"SELECT message_id, chat_id, sender_id, message_text, created_at, updated_at
                            FROM messages
                            WHERE chat_id = @chatId
                            ORDER BY created_at ASC;";
        cmd.Parameters.AddWithValue("@chatId", chatId);

        using var rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            var m = new Message
            {
                Id = rdr.GetInt32("message_id"),
                ChatId = rdr.GetInt32("chat_id"),
                Sender = rdr.GetInt32("sender_id"),
                MessageText = rdr.GetString("message_text"),
                CreatedAt = rdr.IsDBNull(rdr.GetOrdinal("created_at")) ? DateTime.MinValue : rdr.GetDateTime("created_at"),
            };
            result.Add(m);
        }

        CloseConnection();
        return result;
    }

    public bool UserExists(int userId)
    {
        OpenConnection();
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM users WHERE user_id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", userId);

        var found = cmd.ExecuteScalar();
        CloseConnection();
        return found != null && found != DBNull.Value;
    }

    public bool ChatExists(int chatId)
    {
        OpenConnection();
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = "SELECT 1 FROM chats WHERE chat_id = @id LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", chatId);

        var found = cmd.ExecuteScalar();
        CloseConnection();
        return found != null && found != DBNull.Value;
    }

    // Ensures a users row exists for demo or seeded sender ids.
    public void CreateUserIfNotExists(int userId, string name, string email)
    {
        if (UserExists(userId)) return;

        OpenConnection();
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO users (user_id, email, password, name, birth_date, role, is_active, created_at)
                            VALUES (@id, @email, @password, @name, @birthDate, @role, 1, @createdAt);";
        cmd.Parameters.AddWithValue("@id", userId);
        cmd.Parameters.AddWithValue("@email", email ?? $"user{userId}@example.com");
        cmd.Parameters.AddWithValue("@password", "changeme"); // demo: replace with proper hashing in production
        cmd.Parameters.AddWithValue("@name", name ?? $"User{userId}");
        cmd.Parameters.AddWithValue("@birthDate", new DateTime(1990, 1, 1));
        cmd.Parameters.AddWithValue("@role", "User");
        cmd.Parameters.AddWithValue("@createdAt", DateTime.UtcNow);
        cmd.ExecuteNonQuery();
        CloseConnection();
    }

    public int InsertMessage(Message message)
    {
        // validate sender exists to satisfy FK constraint
        if (!UserExists(message.Sender))
            throw new InvalidOperationException($"Sender id {message.Sender} does not exist in users table. Message not inserted.");

        // validate chat exists
        if (!ChatExists(message.ChatId))
            throw new InvalidOperationException($"Chat id {message.ChatId} does not exist. Create or retrieve the chat before inserting messages.");

        OpenConnection();
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = @"INSERT INTO messages (chat_id, sender_id, message_text, created_at)
                            VALUES (@chatId, @senderId, @text, @createdAt);";
        cmd.Parameters.AddWithValue("@chatId", message.ChatId);
        cmd.Parameters.AddWithValue("@senderId", message.Sender);
        cmd.Parameters.AddWithValue("@text", message.MessageText);
        cmd.Parameters.AddWithValue("@createdAt", message.CreatedAt == default ? DateTime.UtcNow : message.CreatedAt);

        cmd.ExecuteNonQuery();

        cmd.CommandText = "SELECT LAST_INSERT_ID();";
        var id = Convert.ToInt32(cmd.ExecuteScalar());
        CloseConnection();
        return id;
    }

    public int GetMatchIdForUser(int userId)
    {
        OpenConnection();
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = @"SELECT match_id
                            FROM matches
                            WHERE user1_id = @id OR user2_id = @id
                            ORDER BY created_at ASC
                            LIMIT 1;";
        cmd.Parameters.AddWithValue("@id", userId);

        var found = cmd.ExecuteScalar();
        CloseConnection();

        return (found != null && found != DBNull.Value) ? Convert.ToInt32(found) : 0;
    }

    /// <summary>
    /// Return the other participant user_id for the given match_id.
    /// Returns 0 when not found or when other user cannot be determined.
    /// </summary>
    public int GetOtherUserIdForMatch(int matchId, int currentUserId)
    {
        OpenConnection();
        using var cmd = Connection.CreateCommand();
        cmd.CommandText = @"SELECT user1_id, user2_id
                            FROM matches
                            WHERE match_id = @matchId
                            LIMIT 1;";
        cmd.Parameters.AddWithValue("@matchId", matchId);

        using var rdr = cmd.ExecuteReader();
        if (!rdr.Read())
        {
            CloseConnection();
            return 0;
        }

        var user1 = rdr.IsDBNull(rdr.GetOrdinal("user1_id")) ? 0 : rdr.GetInt32("user1_id");
        var user2 = rdr.IsDBNull(rdr.GetOrdinal("user2_id")) ? 0 : rdr.GetInt32("user2_id");
        CloseConnection();

        if (currentUserId == 0) return 0;
        if (user1 == currentUserId) return user2;
        if (user2 == currentUserId) return user1;
        return 0;
    }
}