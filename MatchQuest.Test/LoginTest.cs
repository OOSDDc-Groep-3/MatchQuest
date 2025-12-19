using Moq;
using MatchQuest.Core.Services;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using MatchQuest.Core.Helpers;

namespace MatchQuest.Test;

[TestFixture]
public class LoginTest
{
    private Mock<IUserService> _mockUserService;
    private AuthService _authService;
    private User _testUser;

    [SetUp]
    public void Setup()
    {
        _mockUserService = new Mock<IUserService>();
        _authService = new AuthService(_mockUserService.Object);

        // Create a test user with a hashed password
        // Note: Password "testpassword" should be hashed using PasswordHelper.HashPassword
        _testUser = new User(
            id: 1,
            name: "Test User",
            email: "test@example.com",
            password: PasswordHelper.HashPassword("testpassword")
        );
    }

    [Test]
    public void Login_WithValidCredentials_ReturnsUser()
    {
        // Arrange
        string email = "test@example.com";
        string password = "testpassword";
        
        _mockUserService.Setup(s => s.Get(email)).Returns(_testUser);

        // Act
        var result = _authService.Login(email, password);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.Name, Is.EqualTo("Test User"));
        _mockUserService.Verify(s => s.Get(email), Times.Once);
    }

    [Test]
    public void Login_WithInvalidEmail_ReturnsNull()
    {
        // Arrange
        string email = "nonexistent@example.com";
        string password = "anypassword";
        
        _mockUserService.Setup(s => s.Get(email)).Returns((User?)null);

        // Act
        var result = _authService.Login(email, password);

        // Assert
        Assert.That(result, Is.Null);
        _mockUserService.Verify(s => s.Get(email), Times.Once);
    }

    [Test]
    public void Login_WithInvalidPassword_ReturnsNull()
    {
        // Arrange
        string email = "test@example.com";
        string wrongPassword = "wrongpassword";
        
        _mockUserService.Setup(s => s.Get(email)).Returns(_testUser);

        // Act
        var result = _authService.Login(email, wrongPassword);

        // Assert
        Assert.That(result, Is.Null);
        _mockUserService.Verify(s => s.Get(email), Times.Once);
    }

    [Test]
    public void Login_WithEmptyEmail_ReturnsNull()
    {
        // Arrange
        string email = "";
        string password = "testpassword";
        
        _mockUserService.Setup(s => s.Get(email)).Returns((User?)null);

        // Act
        var result = _authService.Login(email, password);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void Login_WithEmptyPassword_ReturnsNull()
    {
        // Arrange
        string email = "test@example.com";
        string password = "";
        
        _mockUserService.Setup(s => s.Get(email)).Returns(_testUser);

        // Act
        var result = _authService.Login(email, password);

        // Assert
        Assert.That(result, Is.Null);
    }
}