using System;
using System.IO;


namespace MatchQuest.Core.Models;

public class Game
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public GameType Type { get; set; }
    public bool Approved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    
    public string Image { get; set; } = string.Empty;
}