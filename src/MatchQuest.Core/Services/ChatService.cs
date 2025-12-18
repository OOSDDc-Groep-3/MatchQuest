using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;

        public ChatService(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
        }

        public int GetOrCreateChatByMatchId(int matchId)
        {
            try
            {
                return _chatRepository.GetOrCreateChatByMatchId(matchId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatService.GetOrCreateChatByMatchId: Exception: {ex}");
                return 0;
            }
        }

        public List<Message> GetMessagesByChatId(int chatId)
        {
            try
            {
                return _chatRepository.GetMessagesByChatId(chatId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatService.GetMessagesByChatId: Exception: {ex}");
                return new List<Message>();
            }
        }

        public int SendMessage(Message message)
        {
            try
            {
                return _chatRepository.InsertMessage(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatService.SendMessage: Exception: {ex}");
                return 0;
            }
        }

        public int GetMatchIdForUser(int userId)
        {
            try
            {
                return _chatRepository.GetMatchIdForUser(userId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatService.GetMatchIdForUser: Exception: {ex}");
                return 0;
            }
        }

        public int GetOtherUserIdForMatch(int matchId, int currentUserId)
        {
            try
            {
                return _chatRepository.GetOtherUserIdForMatch(matchId, currentUserId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatService.GetOtherUserIdForMatch: Exception: {ex}");
                return 0;
            }
        }

        public int GetMatchIdBetween(int userAId, int userBId)
        {
            try
            {
                return _chatRepository.GetMatchIdBetween(userAId, userBId);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatService.GetMatchIdBetween: Exception: {ex}");
                return 0;
            }
        }

        public string? GetLastMessagePreview(int currentUserId, User user)
        {
            try
            {
                if (user == null || user.Id == 0)
                    return null;

                var matchId = GetMatchIdBetween(currentUserId, user.Id);
                if (matchId == 0)
                    return null;

                var chatId = GetOrCreateChatByMatchId(matchId);
                var lastMsg = GetMessagesByChatId(chatId)
                    .OrderByDescending(m => m.CreatedAt)
                    .FirstOrDefault();

                return lastMsg?.MessageText;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ChatService.GetLastMessagePreview: Exception: {ex}");
                return null;
            }
        }
    }
}