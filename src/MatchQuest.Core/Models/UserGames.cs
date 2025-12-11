namespace MatchQuest.Core.Models;

public class UserGame
{
    public int UserGameId { get; set; }
    public int UserId { get; set; }
    public int GameId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}