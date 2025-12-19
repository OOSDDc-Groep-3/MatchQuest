using System;
using System.Collections.Generic;
using MatchQuest.Core.Helpers;
using MatchQuest.Core.Models;
using NUnit.Framework;

namespace MatchQuest.Tests
{
    [TestFixture]
    public class AlgorithmHelperTests
    {
        [Test]
        public void CalculateMatchScore_WithProfilePicture_AddsProfilePictureScore()
        {
            // Arrange
            var user = new User(1, "User1", "user1@test.com", "pass")
            {
                ProfilePicture = "profile.png"
            };
            var matcher = new User(2, "User2", "user2@test.com", "pass");

            // Act
            var result = AlgorithmHelper.CalculateMatchScore(user, matcher);

            // Assert
            Assert.That(result.Score, Is.EqualTo(200));
        }

        [Test]
        public void CalculateMatchScore_WithMatchingGames_AddsMatchingGameScore()
        {
            // Arrange
            var game1 = new Game(1, "Game1", GameType.Adventure, true, "", DateTime.UtcNow, null);
            var game2 = new Game(2, "Game2", GameType.Action, true, "", DateTime.UtcNow, null);

            var user = new User(1, "User1", "user1@test.com", "pass")
            {
                Games = new List<Game> { game1, game2 }
            };
            var matcher = new User(2, "User2", "user2@test.com", "pass")
            {
                Games = new List<Game> { game1 } // matching game
            };

            // Act
            var result = AlgorithmHelper.CalculateMatchScore(user, matcher);

            // Assert
            // 1 matching game = 75 points
            Assert.That(result.Score, Is.EqualTo(125));
        }

        [Test]
        public void CalculateMatchScore_WithMatchingGameTypes_AddsMatchingGameTypeScore()
        {
            // Arrange
            var game1 = new Game(1, "Game1", GameType.Adventure, true, "", DateTime.UtcNow, null);
            var game2 = new Game(2, "Game2", GameType.Action, true, "", DateTime.UtcNow, null);

            var user = new User(1, "User1", "user1@test.com", "pass")
            {
                Games = new List<Game> { game1 }
            };
            var matcher = new User(2, "User2", "user2@test.com", "pass")
            {
                Games = new List<Game> { game2 } // different game but type Action
            };

            // Act
            var result = AlgorithmHelper.CalculateMatchScore(user, matcher);

            // Assert
            // Matching game types = Adventure vs Action => 0 (no match)
            Assert.That(result.Score, Is.EqualTo(0));
        }

        [Test]
        public void CalculateMatchScore_WithAgeGap_AddsAgeGapScore()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var user = new User(1, "User1", "user1@test.com", "pass")
            {
                BirthDate = DateOnly.FromDateTime(today.AddYears(-25))
            };
            var matcher = new User(2, "User2", "user2@test.com", "pass")
            {
                BirthDate = DateOnly.FromDateTime(today.AddYears(-27))
            };

            // Act
            var result = AlgorithmHelper.CalculateMatchScore(user, matcher);

            // Age difference = 2
            // AgeGapScore = Max(0, 100 - (2^2 * 0.2)) = 100 - 0.8 = 99 (rounded)
            Assert.That(result.Score, Is.EqualTo(98));
        }

        [Test]
        public void CalculateMatchScore_FullScenario_CalculatesTotalScore()
        {
            // Arrange
            var today = DateTime.UtcNow;
            var user = new User(1, "User1", "user1@test.com", "pass")
            {
                ProfilePicture = "profile.png",
                BirthDate = DateOnly.FromDateTime(today.AddYears(-30)),
                Games = new List<Game>
                {
                    new Game(1, "Game1", GameType.Adventure, true, "", today, null),
                    new Game(2, "Game2", GameType.Action, true, "", today, null)
                }
            };

            var matcher = new User(2, "User2", "user2@test.com", "pass")
            {
                BirthDate = DateOnly.FromDateTime(today.AddYears(-32)),
                Games = new List<Game>
                {
                    new Game(1, "Game1", GameType.Adventure, true, "", today, null),
                    new Game(3, "Game3", GameType.Rpg, true, "", today, null)
                }
            };

            // Act
            var result = AlgorithmHelper.CalculateMatchScore(user, matcher);

            // Assert
            // ProfilePicture = 200
            // Matching game = Game1 = 75
            // Matching game types = Adventure vs Adventure = 50
            // Age gap = 2 years => 100 - (4*0.2) = 99
            var expectedScore = 349;
            Assert.That(result.Score, Is.EqualTo(expectedScore));
        }
    }
}