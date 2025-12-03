namespace MatchQuest.Core.Models;

public class Chat
{
    public int Id { get; set; }
    public int MatchId { get; set; }
    public List<Message> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
}