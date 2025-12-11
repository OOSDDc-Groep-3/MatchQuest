using MatchQuest.Core.Models;

namespace MatchQuest.Core.Interfaces.Repositories;

public interface IReactionRepository
{
    Reaction? GetReactionByUserIdAndTargetUserId(int userId, int targetUserId);
    Reaction? CreateReaction(int userId, int targetUserId, bool isLike);
}