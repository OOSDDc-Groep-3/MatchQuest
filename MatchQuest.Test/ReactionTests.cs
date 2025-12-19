using System;
using System.Collections.Generic;
using MatchQuest.Core.Models;
using MatchQuest.Core.Services;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Interfaces.Services;
using Moq;
using NUnit.Framework;

namespace MatchQuest.Tests
{
    [TestFixture]
    public class ReactionServiceTests
    {
        private Mock<IReactionRepository> _mockReactionRepository = null!;
        private Mock<IMatchService> _mockMatchService = null!;
        private ReactionService _reactionService = null!;

        [SetUp]
        public void Setup()
        {
            _mockReactionRepository = new Mock<IReactionRepository>();
            _mockMatchService = new Mock<IMatchService>();
            _reactionService = new ReactionService(_mockReactionRepository.Object, _mockMatchService.Object);
        }

        [Test]
        public void CreateReaction_WhenReactionDoesNotExist_CreatesReaction()
        {
            // Arrange
            int userId = 1, targetUserId = 2;
            bool isLike = true;

            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(userId, targetUserId))
                                   .Returns((Reaction?)null);

            var createdReaction = new Reaction(1, userId, targetUserId, isLike, DateTime.UtcNow, null);
            _mockReactionRepository.Setup(r => r.CreateReaction(userId, targetUserId, isLike))
                                   .Returns(createdReaction);

            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(targetUserId, userId))
                                   .Returns((Reaction?)null); // No match yet

            // Act
            var result = _reactionService.CreateReaction(userId, targetUserId, isLike);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result!.UserId);
            Assert.AreEqual(targetUserId, result.TargetUserId);
            Assert.AreEqual(isLike, result.IsLike);
            _mockReactionRepository.Verify(r => r.CreateReaction(userId, targetUserId, isLike), Times.Once);
            _mockMatchService.Verify(m => m.CreateIfNotExists(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void CreateReaction_WhenMatchOccurs_CallsCreateIfNotExists()
        {
            // Arrange
            int userId = 1, targetUserId = 2;
            bool isLike = true;

            var reaction1 = new Reaction(1, userId, targetUserId, true, DateTime.UtcNow, null);
            var reaction2 = new Reaction(2, targetUserId, userId, true, DateTime.UtcNow, null);

            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(userId, targetUserId))
                                   .Returns((Reaction?)null);
            _mockReactionRepository.Setup(r => r.CreateReaction(userId, targetUserId, isLike))
                                   .Returns(reaction1);
            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(targetUserId, userId))
                                   .Returns(reaction2);

            // Act
            var result = _reactionService.CreateReaction(userId, targetUserId, isLike);

            // Assert
            Assert.IsNotNull(result);
            _mockMatchService.Verify(m => m.CreateIfNotExists(userId, targetUserId), Times.Once);
        }

        [Test]
        public void GetReaction_ReturnsReaction()
        {
            // Arrange
            int userId = 1, targetUserId = 2;
            var reaction = new Reaction(1, userId, targetUserId, true, DateTime.UtcNow, null);

            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(userId, targetUserId))
                                   .Returns(reaction);

            // Act
            var result = _reactionService.GetReaction(userId, targetUserId);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(userId, result!.UserId);
            Assert.AreEqual(targetUserId, result.TargetUserId);
        }

        [Test]
        public void ListByUserId_ReturnsAllReactions()
        {
            // Arrange
            int userId = 1;
            var reactions = new List<Reaction>
            {
                new Reaction(1, userId, 2, true, DateTime.UtcNow, null),
                new Reaction(2, userId, 3, false, DateTime.UtcNow, null)
            };

            _mockReactionRepository.Setup(r => r.ListByUserId(userId)).Returns(reactions);

            // Act
            var result = _reactionService.ListByUserId(userId);

            // Assert
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void CheckMatch_WhenBothLiked_ReturnsTrue()
        {
            // Arrange
            int userId = 1, targetUserId = 2;
            var reaction1 = new Reaction(1, userId, targetUserId, true, DateTime.UtcNow, null);
            var reaction2 = new Reaction(2, targetUserId, userId, true, DateTime.UtcNow, null);

            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(userId, targetUserId))
                                   .Returns(reaction1);
            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(targetUserId, userId))
                                   .Returns(reaction2);

            // Act
            var result = _reactionService.CheckMatch(userId, targetUserId);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void CheckMatch_WhenNotBothLiked_ReturnsFalse()
        {
            // Arrange
            int userId = 1, targetUserId = 2;
            var reaction1 = new Reaction(1, userId, targetUserId, true, DateTime.UtcNow, null);
            Reaction? reaction2 = null;

            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(userId, targetUserId))
                                   .Returns(reaction1);
            _mockReactionRepository.Setup(r => r.GetReactionByUserIdAndTargetUserId(targetUserId, userId))
                                   .Returns(reaction2);

            // Act
            var result = _reactionService.CheckMatch(userId, targetUserId);

            // Assert
            Assert.IsFalse(result);
        }
    }
}