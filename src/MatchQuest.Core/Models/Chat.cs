namespace MatchQuest.Core.Models;

public class Chat : Entity
{
    public int MatchId { get; set; }
    public List<Message> Messages { get; set; } = new();
}