using Moq;
using MatchQuest.Core.Services;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;

namespace MatchQuest.Test
{
    [TestFixture]
    public class GameServiceTests
    {
        private Mock<IGameRepository> _mockGameRepository;
        private Mock<IUserGameRepository> _mockUserGameRepository;
        private GameService _gameService;

        private Game _testGame;

        [SetUp]
        public void Setup()
        {
            _mockGameRepository = new Mock<IGameRepository>();
            _mockUserGameRepository = new Mock<IUserGameRepository>();
            _gameService = new GameService(_mockGameRepository.Object, _mockUserGameRepository.Object);

            _testGame = new Game(
                id: 1,
                name: "Test Game",
                type: GameType.Action,
                approved: true,
                image: "test.png",
                createdAt: DateTime.UtcNow,
                updatedAt: null
            );
        }

        [Test]
        public void GetAll_ShouldReturnAllGames()
        {
            // Arrange
            var games = new List<Game> { _testGame };
            _mockGameRepository.Setup(r => r.GetAll()).Returns(games);

            // Act
            var result = _gameService.GetAll();

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Name, Is.EqualTo("Test Game"));
            _mockGameRepository.Verify(r => r.GetAll(), Times.Once);
        }

        [Test]
        public void ListByUserId_ShouldReturnUserGames()
        {
            // Arrange
            int userId = 42;
            var games = new List<Game> { _testGame };
            _mockGameRepository.Setup(r => r.ListByUserId(userId)).Returns(games);

            // Act
            var result = _gameService.ListByUserId(userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Id, Is.EqualTo(_testGame.Id));
            _mockGameRepository.Verify(r => r.ListByUserId(userId), Times.Once);
        }

        [Test]
        public void Get_ShouldReturnGameById()
        {
            // Arrange
            int gameId = 1;
            _mockGameRepository.Setup(r => r.Get(gameId)).Returns(_testGame);

            // Act
            var result = _gameService.Get(gameId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(gameId));
            _mockGameRepository.Verify(r => r.Get(gameId), Times.Once);
        }

        [Test]
        public void AddGameToUser_ShouldReturnGame_WhenAddSucceeds()
        {
            // Arrange
            int userId = 42;
            int gameId = _testGame.Id;
            _mockUserGameRepository.Setup(r => r.AddUserGame(userId, gameId)).Returns(true);
            _mockGameRepository.Setup(r => r.Get(gameId)).Returns(_testGame);

            // Act
            var result = _gameService.AddGameToUser(gameId, userId);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(gameId));
            _mockUserGameRepository.Verify(r => r.AddUserGame(userId, gameId), Times.Once);
            _mockGameRepository.Verify(r => r.Get(gameId), Times.Once);
        }

        [Test]
        public void AddGameToUser_ShouldReturnNull_WhenAddFails()
        {
            // Arrange
            int userId = 42;
            int gameId = _testGame.Id;
            _mockUserGameRepository.Setup(r => r.AddUserGame(userId, gameId)).Returns(false);

            // Act
            var result = _gameService.AddGameToUser(gameId, userId);

            // Assert
            Assert.That(result, Is.Null);
            _mockUserGameRepository.Verify(r => r.AddUserGame(userId, gameId), Times.Once);
            _mockGameRepository.Verify(r => r.Get(It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void RemoveGameFromUser_ShouldReturnTrue_WhenRemoveSucceeds()
        {
            // Arrange
            int userId = 42;
            int gameId = _testGame.Id;
            _mockUserGameRepository.Setup(r => r.RemoveUserGame(userId, gameId)).Returns(true);

            // Act
            var result = _gameService.RemoveGameFromUser(gameId, userId);

            // Assert
            Assert.That(result, Is.True);
            _mockUserGameRepository.Verify(r => r.RemoveUserGame(userId, gameId), Times.Once);
        }

        [Test]
        public void RemoveGameFromUser_ShouldReturnFalse_WhenRemoveFails()
        {
            // Arrange
            int userId = 42;
            int gameId = _testGame.Id;
            _mockUserGameRepository.Setup(r => r.RemoveUserGame(userId, gameId)).Returns(false);

            // Act
            var result = _gameService.RemoveGameFromUser(gameId, userId);

            // Assert
            Assert.That(result, Is.False);
            _mockUserGameRepository.Verify(r => r.RemoveUserGame(userId, gameId), Times.Once);
        }
    }
}