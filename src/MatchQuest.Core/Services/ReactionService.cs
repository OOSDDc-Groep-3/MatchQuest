using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;

namespace MatchQuest.Core.Services;

public class ReactionService : IReactionService
{
    private readonly IReactionRepository _reactionRepository;
    private readonly IMatchService _matchService;
    
    public ReactionService(IReactionRepository reactionRepository, IMatchService matchService)
    {
        _reactionRepository = reactionRepository;
        _matchService = matchService;
    }

    public Reaction? CreateReaction(int userId, int targetUserId, bool isLike)
    {
        var alreadyExists = (GetReaction(userId, targetUserId) != null);
        
        Reaction? reaction = null;
        
        if (!alreadyExists)
        {
            reaction = _reactionRepository.CreateReaction(userId, targetUserId, isLike);
        }

        if (CheckMatch(userId, targetUserId))
        {
            _matchService.CreateIfNotExists(userId, targetUserId);
        }

        return reaction;
    }

    public Reaction? GetReaction(int userId, int targetUserId)
    {
        return _reactionRepository.GetReactionByUserIdAndTargetUserId(userId, targetUserId);
    }
    
    public List<Reaction> ListByUserId(int userId)
    {
        return _reactionRepository.ListByUserId(userId);
    }

    public bool CheckMatch(int userId, int targetUserId)
    {
        var reaction1 = _reactionRepository.GetReactionByUserIdAndTargetUserId(userId, targetUserId);
        var reaction2 = _reactionRepository.GetReactionByUserIdAndTargetUserId(targetUserId, userId);

        // Check if both reactions exists and are likes.
        return (reaction1 != null && reaction2 != null && reaction1.IsLike && reaction2.IsLike);
    }
}