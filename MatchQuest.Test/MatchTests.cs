using System;
using System.Collections.Generic;
using MatchQuest.Core.Models;
using MatchQuest.Core.Services;
using MatchQuest.Core.Interfaces.Repositories;
using Moq;
using NUnit.Framework;

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
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual("Alice", result[0].Name);
            Assert.AreEqual("Bob", result[1].Name);
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
            Assert.IsNotNull(result);
            Assert.AreEqual(userId1, result!.User1Id);
            Assert.AreEqual(userId2, result.User2Id);
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
            Assert.IsNull(result);
            _mockRepository.Verify(r => r.CreateMatch(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }
    }
}