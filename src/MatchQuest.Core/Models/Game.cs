namespace MatchQuest.Core.Models;

public class Game : Entity
{
    public string Name { get; set; } = string.Empty;
    public GameType Type { get; set; }
    public bool Approved { get; set; }
    public string Image { get; set; } = string.Empty;
    
    public Game(int id, string name, GameType type, bool approved, string image, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        Name = name;
        Type = type;
        Approved = approved;
        Image = image;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}