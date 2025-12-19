using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories
{
    public interface IChatRepository
    {
        int GetOrCreateChatByMatchId(int matchId);
        List<Message> GetMessagesByChatId(int chatId);
        bool UserExists(int userId);
        bool ChatExists(int chatId);
        void CreateUserIfNotExists(int userId, string name, string email);
        int InsertMessage(Message message);
        int GetMatchIdForUser(int userId);
        int GetOtherUserIdForMatch(int matchId, int currentUserId);
        int GetMatchIdBetween(int userAId, int userBId);
    }
}