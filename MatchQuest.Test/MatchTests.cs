using System;
using System.Collections.Generic;
using MatchQuest.Core.Models;
using MatchQuest.Core.Services;
using MatchQuest.Core.Interfaces.Repositories;
using Moq;
using NUnit.Framework;
using Match = MatchQuest.Core.Models.Match;

namespace MatchQuest.Tests
{
    [TestFixture]
    public class MatchTests
    {
        private Mock<IMatchRepository> _mockRepository = null!;
        private MatchService _matchService = null!;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IMatchRepository>();
            _matchService = new MatchService(_mockRepository.Object);
        }

        [Test]
        public void GetAllMatchesFromUserId_ReturnsMatches()
        {
            // Arrange
            var userId = 1;
            var matches = new List<User>
            {
                new User(2, "Alice", "alice@test.com", "password"),
                new User(3, "Bob", "bob@test.com", "password")
            };

            _mockRepository.Setup(r => r.GetAllMatchesFromUserId(userId))
                           .Returns(matches);

            // Act
            var result = _matchService.GetAllMatchesFromUserId(userId);

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Alice"));
            Assert.That(result[1].Name, Is.EqualTo("Bob"));
        }

        [Test]
        public void CreateIfNotExists_WhenMatchDoesNotExist_CreatesMatch()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;

            _mockRepository.Setup(r => r.GetByUserIds(userId1, userId2))
                           .Returns((Match?)null);

            var newMatch = new Match(1, userId1, userId2, DateTime.UtcNow, null);
            _mockRepository.Setup(r => r.CreateMatch(userId1, userId2))
                           .Returns(newMatch);

            // Act
            var result = _matchService.CreateIfNotExists(userId1, userId2);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.User1Id, Is.EqualTo(userId1));
            Assert.That(result.User2Id, Is.EqualTo(userId2));
            _mockRepository.Verify(r => r.CreateMatch(userId1, userId2), Times.Once);
        }

        [Test]
        public void CreateIfNotExists_WhenMatchExists_ReturnsNull()
        {
            // Arrange
            var userId1 = 1;
            var userId2 = 2;

            var existingMatch = new Match(1, userId1, userId2, DateTime.UtcNow, null);
            _mockRepository.Setup(r => r.GetByUserIds(userId1, userId2))
                           .Returns(existingMatch);

            // Act
            var result = _matchService.CreateIfNotExists(userId1, userId2);

            // Assert
            Assert.That(result, Is.Null);
            _mockRepository.Verify(r => r.CreateMatch(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}