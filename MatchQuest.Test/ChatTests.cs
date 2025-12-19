using Moq;
using MatchQuest.Core.Services;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Interfaces.Repositories;
using MatchQuest.Core.Models;
using MatchQuest.Core.Helpers;

namespace MatchQuest.Test;

[TestFixture]
public class ChatTests
{
    private Mock<IChatRepository> _mockChatRepository;
    private ChatService _chatService;
    private User _testUser1;
    private User _testUser2;
    private int _testMatchId;
    private int _testChatId;

    [SetUp]
    public void Setup()
    {
        _mockChatRepository = new Mock<IChatRepository>();
        _chatService = new ChatService(_mockChatRepository.Object);

        _testMatchId = 1;
        _testChatId = 100;

        // Create test users
        _testUser1 = new User(
            id: 1,
            name: "User One",
            email: "user1@example.com",
            password: PasswordHelper.HashPassword("password123")
        );

        _testUser2 = new User(
            id: 2,
            name: "User Two",
            email: "user2@example.com",
            password: PasswordHelper.HashPassword("password123")
        );
    }

    [Test]
    public void GetOrCreateChatByMatchId_WhenChatExists_ReturnsExistingChatId()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetOrCreateChatByMatchId(_testMatchId))
            .Returns(_testChatId);

        // Act
        var result = _chatService.GetOrCreateChatByMatchId(_testMatchId);

        // Assert
        Assert.That(result, Is.EqualTo(_testChatId));
        _mockChatRepository.Verify(r => r.GetOrCreateChatByMatchId(_testMatchId), Times.Once);
    }

    [Test]
    public void GetOrCreateChatByMatchId_WhenChatDoesNotExist_CreatesAndReturnsNewChatId()
    {
        // Arrange
        int newChatId = 101;
        _mockChatRepository.Setup(r => r.GetOrCreateChatByMatchId(_testMatchId))
            .Returns(newChatId);

        // Act
        var result = _chatService.GetOrCreateChatByMatchId(_testMatchId);

        // Assert
        Assert.That(result, Is.EqualTo(newChatId));
        _mockChatRepository.Verify(r => r.GetOrCreateChatByMatchId(_testMatchId), Times.Once);
    }

    [Test]
    public void SendMessage_WithValidMessage_ReturnsMessageId()
    {
        // Arrange
        var message = new Message
        {
            ChatId = _testChatId,
            SenderId = _testUser1.Id,
            MessageText = "Hello, how are you?",
            CreatedAt = DateTime.UtcNow
        };

        int expectedMessageId = 1;
        _mockChatRepository.Setup(r => r.InsertMessage(It.IsAny<Message>()))
            .Returns(expectedMessageId);

        // Act
        var result = _chatService.SendMessage(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessageId));
        _mockChatRepository.Verify(r => r.InsertMessage(It.Is<Message>(m =>
            m.ChatId == _testChatId &&
            m.SenderId == _testUser1.Id &&
            m.MessageText == "Hello, how are you?"
        )), Times.Once);
    }

    [Test]
    public void SendMessage_WithEmptyText_StillSendsMessage()
    {
        // Arrange
        var message = new Message
        {
            ChatId = _testChatId,
            SenderId = _testUser1.Id,
            MessageText = "",
            CreatedAt = DateTime.UtcNow
        };

        int expectedMessageId = 2;
        _mockChatRepository.Setup(r => r.InsertMessage(It.IsAny<Message>()))
            .Returns(expectedMessageId);

        // Act
        var result = _chatService.SendMessage(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessageId));
        _mockChatRepository.Verify(r => r.InsertMessage(It.IsAny<Message>()), Times.Once);
    }

    [Test]
    public void GetMessagesByChatId_WithMessages_ReturnsMessageList()
    {
        // Arrange
        var messages = new List<Message>
        {
            new Message
            {
                Id = 1,
                ChatId = _testChatId,
                SenderId = _testUser1.Id,
                MessageText = "Hello!",
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new Message
            {
                Id = 2,
                ChatId = _testChatId,
                SenderId = _testUser2.Id,
                MessageText = "Hi there!",
                CreatedAt = DateTime.UtcNow.AddMinutes(-3)
            },
            new Message
            {
                Id = 3,
                ChatId = _testChatId,
                SenderId = _testUser1.Id,
                MessageText = "How are you?",
                CreatedAt = DateTime.UtcNow
            }
        };

        _mockChatRepository.Setup(r => r.GetMessagesByChatId(_testChatId))
            .Returns(messages);

        // Act
        var result = _chatService.GetMessagesByChatId(_testChatId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].MessageText, Is.EqualTo("Hello!"));
        Assert.That(result[1].MessageText, Is.EqualTo("Hi there!"));
        Assert.That(result[2].MessageText, Is.EqualTo("How are you?"));
        _mockChatRepository.Verify(r => r.GetMessagesByChatId(_testChatId), Times.Once);
    }

    [Test]
    public void GetMessagesByChatId_WithNoMessages_ReturnsEmptyList()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMessagesByChatId(_testChatId))
            .Returns(new List<Message>());

        // Act
        var result = _chatService.GetMessagesByChatId(_testChatId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(0));
        _mockChatRepository.Verify(r => r.GetMessagesByChatId(_testChatId), Times.Once);
    }

    [Test]
    public void GetMatchIdForUser_WithExistingMatch_ReturnsMatchId()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMatchIdForUser(_testUser1.Id))
            .Returns(_testMatchId);

        // Act
        var result = _chatService.GetMatchIdForUser(_testUser1.Id);

        // Assert
        Assert.That(result, Is.EqualTo(_testMatchId));
        _mockChatRepository.Verify(r => r.GetMatchIdForUser(_testUser1.Id), Times.Once);
    }

    [Test]
    public void GetMatchIdForUser_WithNoMatch_ReturnsZero()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMatchIdForUser(_testUser1.Id))
            .Returns(0);

        // Act
        var result = _chatService.GetMatchIdForUser(_testUser1.Id);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        _mockChatRepository.Verify(r => r.GetMatchIdForUser(_testUser1.Id), Times.Once);
    }

    [Test]
    public void GetOtherUserIdForMatch_ReturnsOtherUserId()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetOtherUserIdForMatch(_testMatchId, _testUser1.Id))
            .Returns(_testUser2.Id);

        // Act
        var result = _chatService.GetOtherUserIdForMatch(_testMatchId, _testUser1.Id);

        // Assert
        Assert.That(result, Is.EqualTo(_testUser2.Id));
        _mockChatRepository.Verify(r => r.GetOtherUserIdForMatch(_testMatchId, _testUser1.Id), Times.Once);
    }

    [Test]
    public void GetMatchIdBetween_WithExistingMatch_ReturnsMatchId()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMatchIdBetween(_testUser1.Id, _testUser2.Id))
            .Returns(_testMatchId);

        // Act
        var result = _chatService.GetMatchIdBetween(_testUser1.Id, _testUser2.Id);

        // Assert
        Assert.That(result, Is.EqualTo(_testMatchId));
        _mockChatRepository.Verify(r => r.GetMatchIdBetween(_testUser1.Id, _testUser2.Id), Times.Once);
    }

    [Test]
    public void GetMatchIdBetween_WithReversedUserIds_ReturnsMatchId()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMatchIdBetween(_testUser2.Id, _testUser1.Id))
            .Returns(_testMatchId);

        // Act
        var result = _chatService.GetMatchIdBetween(_testUser2.Id, _testUser1.Id);

        // Assert
        Assert.That(result, Is.EqualTo(_testMatchId));
        _mockChatRepository.Verify(r => r.GetMatchIdBetween(_testUser2.Id, _testUser1.Id), Times.Once);
    }

    [Test]
    public void GetMatchIdBetween_WithNoMatch_ReturnsZero()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMatchIdBetween(_testUser1.Id, _testUser2.Id))
            .Returns(0);

        // Act
        var result = _chatService.GetMatchIdBetween(_testUser1.Id, _testUser2.Id);

        // Assert
        Assert.That(result, Is.EqualTo(0));
        _mockChatRepository.Verify(r => r.GetMatchIdBetween(_testUser1.Id, _testUser2.Id), Times.Once);
    }

    [Test]
    public void GetLastMessagePreview_WithNoMessages_ReturnsNull()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMatchIdBetween(_testUser1.Id, _testUser2.Id))
            .Returns(_testMatchId);
        _mockChatRepository.Setup(r => r.GetOrCreateChatByMatchId(_testMatchId))
            .Returns(_testChatId);
        _mockChatRepository.Setup(r => r.GetMessagesByChatId(_testChatId))
            .Returns(new List<Message>());

        // Act
        var result = _chatService.GetLastMessagePreview(_testUser1.Id, _testUser2);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLastMessagePreview_WithNoMatch_ReturnsNull()
    {
        // Arrange
        _mockChatRepository.Setup(r => r.GetMatchIdBetween(_testUser1.Id, _testUser2.Id))
            .Returns(0);

        // Act
        var result = _chatService.GetLastMessagePreview(_testUser1.Id, _testUser2);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLastMessagePreview_WithNullUser_ReturnsNull()
    {
        // Arrange & Act
        var result = _chatService.GetLastMessagePreview(_testUser1.Id, null);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Message_CreatedAt_IsSetCorrectly()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var message = new Message
        {
            ChatId = _testChatId,
            SenderId = _testUser1.Id,
            MessageText = "Test message",
            CreatedAt = DateTime.UtcNow
        };
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.That(message.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(message.CreatedAt, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void SendMessage_WithLongText_SendsSuccessfully()
    {
        // Arrange
        string longText = new string('A', 1000);
        var message = new Message
        {
            ChatId = _testChatId,
            SenderId = _testUser1.Id,
            MessageText = longText,
            CreatedAt = DateTime.UtcNow
        };

        int expectedMessageId = 3;
        _mockChatRepository.Setup(r => r.InsertMessage(It.IsAny<Message>()))
            .Returns(expectedMessageId);

        // Act
        var result = _chatService.SendMessage(message);

        // Assert
        Assert.That(result, Is.EqualTo(expectedMessageId));
        _mockChatRepository.Verify(r => r.InsertMessage(It.Is<Message>(m =>
            m.MessageText.Length == 1000
        )), Times.Once);
    }

    [Test]
    public void GetMessagesByChatId_MessagesOrderedByCreatedAt()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var messages = new List<Message>
        {
            new Message
            {
                Id = 1,
                ChatId = _testChatId,
                SenderId = _testUser1.Id,
                MessageText = "First",
                CreatedAt = now.AddMinutes(-10)
            },
            new Message
            {
                Id = 2,
                ChatId = _testChatId,
                SenderId = _testUser2.Id,
                MessageText = "Second",
                CreatedAt = now.AddMinutes(-5)
            },
            new Message
            {
                Id = 3,
                ChatId = _testChatId,
                SenderId = _testUser1.Id,
                MessageText = "Third",
                CreatedAt = now
            }
        };

        _mockChatRepository.Setup(r => r.GetMessagesByChatId(_testChatId))
            .Returns(messages);

        // Act
        var result = _chatService.GetMessagesByChatId(_testChatId);

        // Assert
        Assert.That(result[0].CreatedAt, Is.LessThan(result[1].CreatedAt));
        Assert.That(result[1].CreatedAt, Is.LessThan(result[2].CreatedAt));
    }
}