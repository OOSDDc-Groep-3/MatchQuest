namespace MatchQuest.Core.Models;

public class Match
{
    public int Id { get; set; }
    public int User1 { get; set; }
    public int User2 { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
}