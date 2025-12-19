using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services
{
    public interface IChatService
    {
        int GetOrCreateChatByMatchId(int matchId);
        List<Message> GetMessagesByChatId(int chatId);
        int SendMessage(Message message);
        int GetMatchIdForUser(int userId);
        int GetOtherUserIdForMatch(int matchId, int currentUserId);
        int GetMatchIdBetween(int userAId, int userBId);
        string? GetLastMessagePreview(int currentUserId, User user);
    }
}