namespace MatchQuest.Core.Models;

public class Reaction : Entity
{
    public int UserId { get; set; } // User who liked
    public int TargetUserId { get; set; } // The user who is liked
    public bool IsLike { get; set; } // true = like, false = dislike

    public Reaction(int userId, int targetUserId, bool isLike)
    {
        UserId = userId;
        TargetUserId = targetUserId;
        IsLike = isLike;
    }
    
    public Reaction(int id, int userId, int targetUserId, bool isLike, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        UserId = userId;
        TargetUserId = targetUserId;
        IsLike = isLike;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}