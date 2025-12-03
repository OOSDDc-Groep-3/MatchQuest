namespace MatchQuest.Core.Models;

public class Message
{
    public int Id { get; set; }
    public int ChatId { get; set; }
    public int Sender { get; set; }
    public string MessageText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }

    // UI-only flag - set by the viewmodel so XAML can show left/right correctly.
    // Not persisted to DB.
    public bool IsOutbound { get; set; }
}