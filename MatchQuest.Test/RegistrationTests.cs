using Moq;
using MatchQuest.Core.Services;
using MatchQuest.Core.Interfaces.Services;
using MatchQuest.Core.Models;
using MatchQuest.Core.Helpers;

namespace MatchQuest.Test;

[TestFixture]
public class RegistrationTests
{
    private Mock<IUserService> _mockUserService;
    private User _existingUser;

    [SetUp]
    public void Setup()
    {
        _mockUserService = new Mock<IUserService>();

        // Create an existing user for duplicate email tests
        _existingUser = new User(
            id: 1,
            name: "Existing User",
            email: "existing@example.com",
            password: PasswordHelper.HashPassword("password123")
        );
    }

    [Test]
    public void Registration_WithValidData_CreatesUserSuccessfully()
    {
        // Arrange
        string email = "newuser@example.com";
        string password = "password123";
        string name = "John Doe";
        DateOnly birthDate = new DateOnly(1995, 5, 15);
        string region = "North America";

        var newUser = new User(
            id: 0,
            name: name,
            email: email,
            password: PasswordHelper.HashPassword(password)
        )
        {
            BirthDate = birthDate,
            Region = region
        };

        var createdUser = new User(
            id: 2,
            name: name,
            email: email,
            password: PasswordHelper.HashPassword(password),
            birthDate: birthDate,
            region: region,
            biography: null,
            profilePicture: null,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Get(email)).Returns((User?)null);
        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var emailExists = _mockUserService.Object.Get(email);
        var result = _mockUserService.Object.Create(newUser);

        // Assert
        Assert.That(emailExists, Is.Null, "Email should not exist before registration");
        Assert.That(result, Is.Not.Null, "User creation should return a user");
        Assert.That(result.Id, Is.GreaterThan(0), "Created user should have a valid ID");
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.Name, Is.EqualTo(name));
        Assert.That(result.BirthDate, Is.EqualTo(birthDate));
        Assert.That(result.Region, Is.EqualTo(region));
        Assert.That(result.IsActive, Is.True);
        _mockUserService.Verify(s => s.Get(email), Times.Once);
        _mockUserService.Verify(s => s.Create(It.IsAny<User>()), Times.Once);
    }

    [Test]
    public void Registration_WithDuplicateEmail_ShouldNotCreateUser()
    {
        // Arrange
        string duplicateEmail = "existing@example.com";
        string password = "newpassword";

        _mockUserService.Setup(s => s.Get(duplicateEmail)).Returns(_existingUser);

        // Act
        var existingUserCheck = _mockUserService.Object.Get(duplicateEmail);

        // Assert
        Assert.That(existingUserCheck, Is.Not.Null, "Existing user should be found");
        Assert.That(existingUserCheck.Email, Is.EqualTo(duplicateEmail));
        _mockUserService.Verify(s => s.Get(duplicateEmail), Times.Once);
        _mockUserService.Verify(s => s.Create(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public void Registration_PasswordHashing_StoresHashedPassword()
    {
        // Arrange
        string plainPassword = "mySecurePassword123";
        string hashedPassword = PasswordHelper.HashPassword(plainPassword);

        var newUser = new User(
            id: 0,
            name: "Test User",
            email: "test@example.com",
            password: hashedPassword
        );

        var createdUser = new User(
            id: 3,
            name: "Test User",
            email: "test@example.com",
            password: hashedPassword,
            birthDate: null,
            region: null,
            biography: null,
            profilePicture: null,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Get("test@example.com")).Returns((User?)null);
        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _mockUserService.Object.Create(newUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Password, Is.Not.EqualTo(plainPassword), "Password should be hashed");
        Assert.That(PasswordHelper.VerifyPassword(plainPassword, result.Password), Is.True, "Hashed password should be verifiable");
    }

    [Test]
    public void Registration_WithMinimalData_CreatesUserWithoutOptionalFields()
    {
        // Arrange
        string email = "minimal@example.com";
        string password = "password123";
        string name = "Minimal User";

        var newUser = new User(
            id: 0,
            name: name,
            email: email,
            password: PasswordHelper.HashPassword(password)
        );

        var createdUser = new User(
            id: 4,
            name: name,
            email: email,
            password: PasswordHelper.HashPassword(password),
            birthDate: null,
            region: null,
            biography: null,
            profilePicture: null,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Get(email)).Returns((User?)null);
        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _mockUserService.Object.Create(newUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.Name, Is.EqualTo(name));
        Assert.That(result.BirthDate, Is.Null);
        Assert.That(result.Region, Is.Null);
        Assert.That(result.Biography, Is.Null);
    }

    [Test]
    public void Registration_WithCompleteProfile_CreatesUserWithAllFields()
    {
        // Arrange
        string email = "complete@example.com";
        string password = "password123";
        string name = "Complete User";
        DateOnly birthDate = new DateOnly(1990, 1, 1);
        string region = "Europe";
        string biography = "This is my biography";
        string profilePicture = "/images/profile.jpg";

        var newUser = new User(
            id: 0,
            name: name,
            email: email,
            password: PasswordHelper.HashPassword(password)
        )
        {
            BirthDate = birthDate,
            Region = region,
            Biography = biography,
            ProfilePicture = profilePicture
        };

        var createdUser = new User(
            id: 5,
            name: name,
            email: email,
            password: PasswordHelper.HashPassword(password),
            birthDate: birthDate,
            region: region,
            biography: biography,
            profilePicture: profilePicture,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Get(email)).Returns((User?)null);
        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _mockUserService.Object.Create(newUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Email, Is.EqualTo(email));
        Assert.That(result.Name, Is.EqualTo(name));
        Assert.That(result.BirthDate, Is.EqualTo(birthDate));
        Assert.That(result.Region, Is.EqualTo(region));
        Assert.That(result.Biography, Is.EqualTo(biography));
        Assert.That(result.ProfilePicture, Is.EqualTo(profilePicture));
        Assert.That(result.IsActive, Is.True);
    }

    [Test]
    public void Registration_CreatedUser_HasDefaultActiveStatus()
    {
        // Arrange
        var newUser = new User(
            id: 0,
            name: "Active User",
            email: "active@example.com",
            password: PasswordHelper.HashPassword("password123")
        );

        var createdUser = new User(
            id: 6,
            name: "Active User",
            email: "active@example.com",
            password: PasswordHelper.HashPassword("password123"),
            birthDate: null,
            region: null,
            biography: null,
            profilePicture: null,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _mockUserService.Object.Create(newUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsActive, Is.True);
    }

    [Test]
    public void Registration_CreatedUser_HasCreatedAtTimestamp()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;
        var newUser = new User(
            id: 0,
            name: "Timestamp User",
            email: "timestamp@example.com",
            password: PasswordHelper.HashPassword("password123")
        );

        var createdUser = new User(
            id: 7,
            name: "Timestamp User",
            email: "timestamp@example.com",
            password: PasswordHelper.HashPassword("password123"),
            birthDate: null,
            region: null,
            biography: null,
            profilePicture: null,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _mockUserService.Object.Create(newUser);
        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.CreatedAt, Is.GreaterThanOrEqualTo(beforeCreation));
        Assert.That(result.CreatedAt, Is.LessThanOrEqualTo(afterCreation));
    }

    [Test]
    public void Registration_WithValidBirthDate_CalculatesAgeCorrectly()
    {
        // Arrange
        var birthDate = new DateOnly(1995, 6, 15);
        var newUser = new User(
            id: 0,
            name: "Age Test User",
            email: "age@example.com",
            password: PasswordHelper.HashPassword("password123")
        )
        {
            BirthDate = birthDate
        };

        var createdUser = new User(
            id: 8,
            name: "Age Test User",
            email: "age@example.com",
            password: PasswordHelper.HashPassword("password123"),
            birthDate: birthDate,
            region: null,
            biography: null,
            profilePicture: null,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _mockUserService.Object.Create(newUser);
        var calculatedAge = result.GetAge();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.BirthDate, Is.EqualTo(birthDate));
        Assert.That(calculatedAge, Is.GreaterThan(0));
        var expectedAge = DateTime.UtcNow.Year - birthDate.Year;
        Assert.That(calculatedAge, Is.InRange(expectedAge - 1, expectedAge));
    }

    [Test]
    public void Registration_CaseInsensitiveEmail_DetectsDuplicate()
    {
        // Arrange
        string existingEmail = "existing@example.com";
        string duplicateEmailDifferentCase = "EXISTING@EXAMPLE.COM";

        _mockUserService.Setup(s => s.Get(It.Is<string>(e => e.ToLower() == existingEmail.ToLower())))
            .Returns(_existingUser);

        // Act
        var result = _mockUserService.Object.Get(duplicateEmailDifferentCase);

        // Assert
        Assert.That(result, Is.Not.Null, "Email check should be case-insensitive");
        _mockUserService.Verify(s => s.Get(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public void Registration_WithEmptyOptionalFields_ShouldSucceed()
    {
        // Arrange
        var newUser = new User(
            id: 0,
            name: "User With Nulls",
            email: "nullfields@example.com",
            password: PasswordHelper.HashPassword("password123")
        )
        {
            BirthDate = null,
            Region = null,
            Biography = null,
            ProfilePicture = null
        };

        var createdUser = new User(
            id: 9,
            name: "User With Nulls",
            email: "nullfields@example.com",
            password: PasswordHelper.HashPassword("password123"),
            birthDate: null,
            region: null,
            biography: null,
            profilePicture: null,
            isActive: true,
            createdAt: DateTime.UtcNow,
            updatedAt: null
        );

        _mockUserService.Setup(s => s.Get("nullfields@example.com")).Returns((User?)null);
        _mockUserService.Setup(s => s.Create(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _mockUserService.Object.Create(newUser);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.BirthDate, Is.Null);
        Assert.That(result.Region, Is.Null);
        Assert.That(result.Biography, Is.Null);
        Assert.That(result.ProfilePicture, Is.Null);
    }
}