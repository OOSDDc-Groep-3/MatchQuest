namespace MatchQuest.Core.Models;

public class MatchingScore
{
    public User User { get; set; }
    public User Matcher { get; set; }
    public int Score { get; set; } = 0;
    
    public MatchingScore(User user, User matcher)
    {
        User = user;
        Matcher = matcher;
    }
}