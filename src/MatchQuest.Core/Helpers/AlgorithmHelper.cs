using MatchQuest.Core.Models;
using System.Diagnostics;

namespace MatchQuest.Core.Helpers;

public static class AlgorithmHelper
{
    private static readonly int AgeGapMaxScore = 100;
    private static readonly double AgeGapFactor = 0.2;
    private static readonly int MatchingGameScore = 75;
    private static readonly int MatchingGameTypeScore = 50;
    private static readonly int ProfilePictureScore = 200;
    
    public static MatchingScore CalculateMatchScore(User user, User matcher)
    {
        var matchingScore = new MatchingScore(user, user);

        // Has profile picture
        if (!string.IsNullOrEmpty(user.ProfilePicture))
        {
            matchingScore.Score += ProfilePictureScore;
        }
        
        // Matching game - 75 points
        var matchingGame = user.Games.Intersect(matcher.Games).ToList();
        matchingScore.Score += matchingGame.Count * MatchingGameScore;
        
        // Matching game types - 50 points
        var matcherGameTypes = matcher.Games.Select(g => g.Type).ToList();
        var userGameTypes = user.Games.Select(g => g.Type).ToList();
        var matchingGameTypes = userGameTypes.Intersect(matcherGameTypes).ToList();
        matchingScore.Score += matchingGameTypes.Count * MatchingGameTypeScore;
        
        // Age gap score
        if (user.BirthDate.HasValue && matcher.BirthDate.HasValue)
        {
            matchingScore.Score += CalculateAgeGapScore(user.BirthDate.Value, matcher.BirthDate.Value);
        }

        return matchingScore;
    }

    private static int CalculateAgeGapScore(DateOnly userDob, DateOnly matchDob)
    {
        // get the exact age of user
        var userAge = DateTime.Now.Year - userDob.Year;
        if (DateTime.Now.DayOfYear < userDob.DayOfYear) userAge--;
        
        var matcherAge = DateTime.Now.Year - matchDob.Year;
        if (DateTime.Now.DayOfYear < matchDob.DayOfYear) matcherAge--;
        
        var ageDifference = Math.Abs(userAge - matcherAge);
        
        // [Max(0,100-(5^2 * 0.2))]
        var ageGapScore = Math.Max(0, AgeGapMaxScore - (Math.Pow(ageDifference, 2) * AgeGapFactor));
        return (int)Math.Round(ageGapScore);
    }
}