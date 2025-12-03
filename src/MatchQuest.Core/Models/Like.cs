namespace MatchQuest.Core.Models;

public class Like
{
    public int Id { get; set; }
    public int LikerId { get; set; } // User who liked
    public int LikeeId { get; set; } // The user who is liked
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
}