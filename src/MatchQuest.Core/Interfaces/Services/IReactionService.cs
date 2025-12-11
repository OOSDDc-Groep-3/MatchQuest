using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Services;

public interface IReactionService
{
    Reaction? CreateReaction(int userId, int targetUserId, bool isLike);
    Reaction? GetReaction(int userId, int targetUserId);
}