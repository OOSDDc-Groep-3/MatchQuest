using MatchQuest.Core.Models;
using System.Diagnostics;

namespace MatchQuest.Core.Helpers;

public class AlgorithmHelper
{
    private readonly int _ageGapMaxScore = 100;
    private readonly double _ageGapFactor = 0.2;
    private readonly int _matchingGameScore = 75;
    private readonly int _matchingGameTypeScore = 50;
    private readonly int _profilePictureScore = 200;
    
    public MatchingScore CalculateMatchScore(User user, User matcher)
    {
        var matchingScore = new MatchingScore(user, user);

        // Has profile picture
        if (!string.IsNullOrEmpty(matcher.ProfilePicture))
        {
            matchingScore.Score += _profilePictureScore;
        }
        
        // Matching game - 75 points
        var matchingGame = user.Games.Intersect(matcher.Games).ToList();
        matchingScore.Score += matchingGame.Count * _matchingGameScore;
        
        // Matching game types - 50 points
        var matcherGameTypes = matcher.Games.Select(g => g.Type).ToList();
        var userGameTypes = user.Games.Select(g => g.Type).ToList();
        var matchingGameTypes = userGameTypes.Intersect(matcherGameTypes).ToList();
        matchingScore.Score += matchingGameTypes.Count * _matchingGameTypeScore;
        
        // Age gap score
        if (user.BirthDate.HasValue && matcher.BirthDate.HasValue)
        {
            matchingScore.Score += CalculateAgeGapScore(user.BirthDate.Value, matcher.BirthDate.Value);
        }

        return matchingScore;
    }

    private int CalculateAgeGapScore(DateOnly userDob, DateOnly matchDob)
    {
        // get the exact age of user
        var userAge = DateTime.Now.Year - userDob.Year;
        if (DateTime.Now.DayOfYear < userDob.DayOfYear) userAge--;
        
        var matcherAge = DateTime.Now.Year - matchDob.Year;
        if (DateTime.Now.DayOfYear < matchDob.DayOfYear) matcherAge--;
        
        var ageDifference = Math.Abs(userAge - matcherAge);
        
        // [Max(0,100-(5^2 * 0.2))]
        var ageGapScore = Math.Max(0, _ageGapMaxScore - (Math.Pow(ageDifference, 2) * _ageGapFactor));
        return (int)Math.Round(ageGapScore);
    }
}