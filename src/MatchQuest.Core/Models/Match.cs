namespace MatchQuest.Core.Models;

public class Match : Entity
{
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    
    public Match(int id, int user1Id, int user2Id, DateTime createdAt, DateTime? updatedAt)
    {
        Id = id;
        User1Id = user1Id;
        User2Id = user2Id;
        CreatedAt = createdAt;
        UpdatedAt = updatedAt;
    }
}