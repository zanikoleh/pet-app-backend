using Xunit;
using FluentAssertions;
using Moq;
using AutoMapper;
using IdentityService.Application.Commands;
using IdentityService.Application.DTOs;
using IdentityService.Application.Handlers;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.ValueObjects;
using SharedKernel;

namespace IdentityService.Application.Tests.Handlers;

public class AuthenticationCommandHandlersTests
{
    #region Setup

    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    public AuthenticationCommandHandlersTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _mapperMock = new Mock<IMapper>();
    }

    private void SetupMapper(User user)
    {
        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email.Value,
            FullName = GetFullName(user),
            IsEmailVerified = user.IsEmailConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            LastLoginAt = user.LastLoginAt,
            OAuthLinks = user.OAuthProviders
                .Select(op => new OAuthLinkDto
                {
                    Provider = op.Provider,
                    LinkedAt = op.LinkedAt
                })
                .ToList()
        };

        _mapperMock
            .Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns(userDto);
    }

    private static string? GetFullName(User user)
    {
        var firstName = user.GetType().GetProperty("FirstName")?.GetValue(user) as string;
        var lastName = user.GetType().GetProperty("LastName")?.GetValue(user) as string;
        
        if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(lastName))
            return null;
        
        return $"{firstName} {lastName}".Trim();
    }

    #endregion

    #region Register Command Handler Tests

    [Fact]
    public async Task Handle_RegisterCommand_WithValidData_ShouldSucceed()
    {
        // Arrange
        const string email = "newuser@example.com";
        const string password = "SecurePassword123!";
        const string fullName = "John Doe";

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), email, It.IsAny<string>()))
            .Returns("access_token_123");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token_123");

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpirationMinutes())
            .Returns(10080);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpirationMinutes())
            .Returns(15);

        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new RegisterCommand(email, password, fullName);

        // Setup mapper to return UserDto
        _mapperMock
            .Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns((User u) =>
            {
                return new UserDto
                {
                    Id = u.Id,
                    Email = u.Email.Value,
                    FullName = GetFullName(u),
                    IsEmailVerified = u.IsEmailConfirmed,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt,
                    OAuthLinks = u.OAuthProviders
                        .Select(op => new OAuthLinkDto
                        {
                            Provider = op.Provider,
                            LinkedAt = op.LinkedAt
                        })
                        .ToList()
                };
            });

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(email);
        result.AccessToken.Should().Be("access_token_123");
        result.RefreshToken.Should().Be("refresh_token_123");
        result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(15), TimeSpan.FromSeconds(5));

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Once);
        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_RegisterCommand_WithExistingEmail_ShouldThrowBusinessLogicException()
    {
        // Arrange
        const string email = "existing@example.com";
        const string password = "Password123!";

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new RegisterCommand(email, password, null);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<BusinessLogicException>()
            .WithMessage("*already registered*");
    }

    [Fact]
    public async Task Handle_RegisterCommand_ShouldAddRefreshToken()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "Password123!";
        const int refreshTokenExpiration = 10080;

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access_token");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpirationMinutes())
            .Returns(refreshTokenExpiration);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpirationMinutes())
            .Returns(15);

        var handler = new RegisterCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new RegisterCommand(email, password, "Test User");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    #endregion

    #region Login Command Handler Tests

    [Fact]
    public async Task Handle_LoginCommand_WithValidCredentials_ShouldSucceed()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "CorrectPassword123!";

        var user = new User(
            Email.Create(email),
            PasswordHash.Create(password),
            "Test User");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(user.Id, email, It.IsAny<string>()))
            .Returns("access_token");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpirationMinutes())
            .Returns(10080);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpirationMinutes())
            .Returns(15);

        _mapperMock
            .Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns((User u) =>
            {
                return new UserDto
                {
                    Id = u.Id,
                    Email = u.Email.Value,
                    FullName = GetFullName(u),
                    IsEmailVerified = u.IsEmailConfirmed,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt,
                    OAuthLinks = u.OAuthProviders
                        .Select(op => new OAuthLinkDto
                        {
                            Provider = op.Provider,
                            LinkedAt = op.LinkedAt
                        })
                        .ToList()
                };
            });

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new LoginCommand(email, password);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(email);
        result.AccessToken.Should().Be("access_token");
        result.RefreshToken.Should().Be("refresh_token");
    }

    [Fact]
    public async Task Handle_LoginCommand_WithInvalidEmail_ShouldThrowNotFoundException()
    {
        // Arrange
        const string email = "nonexistent@example.com";
        const string password = "Password123!";

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new LoginCommand(email, password);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_LoginCommand_WithWrongPassword_ShouldThrowNotFoundException()
    {
        // Arrange
        const string email = "user@example.com";
        const string correctPassword = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword123!";

        var user = new User(
            Email.Create(email),
            PasswordHash.Create(correctPassword),
            "Test User");

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new LoginCommand(email, wrongPassword);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_LoginCommand_WithDeactivatedAccount_ShouldThrowBusinessLogicException()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "Password123!";

        var user = new User(
            Email.Create(email),
            PasswordHash.Create(password),
            "Test User");
        user.Deactivate();

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new LoginCommand(email, password);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<BusinessLogicException>()
            .WithMessage("*deactivated*");
    }

    [Fact]
    public async Task Handle_LoginCommand_ShouldRecordLoginTime()
    {
        // Arrange
        const string email = "user@example.com";
        const string password = "Password123!";

        var user = new User(
            Email.Create(email),
            PasswordHash.Create(password),
            "Test User");
        var initialLastLogin = user.LastLoginAt;

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("token");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpirationMinutes())
            .Returns(10080);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpirationMinutes())
            .Returns(15);

        var handler = new LoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new LoginCommand(email, password);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.LastLoginAt.Should().BeAfter(initialLastLogin);
    }

    #endregion

    #region Refresh Token Command Handler Tests

    [Fact]
    public async Task Handle_RefreshAccessTokenCommand_WithValidToken_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string refreshToken = "valid_refresh_token";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        var user = new User(Email.Create("user@example.com"), null);
        user.AddRefreshToken(refreshToken, 10080);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(userId, It.IsAny<string>(), It.IsAny<string>()))
            .Returns("new_access_token");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns(refreshToken);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpirationMinutes())
            .Returns(15);

        _mapperMock
            .Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns((User u) =>
            {
                return new UserDto
                {
                    Id = u.Id,
                    Email = u.Email.Value,
                    FullName = GetFullName(u),
                    IsEmailVerified = u.IsEmailConfirmed,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt,
                    OAuthLinks = u.OAuthProviders
                        .Select(op => new OAuthLinkDto
                        {
                            Provider = op.Provider,
                            LinkedAt = op.LinkedAt
                        })
                        .ToList()
                };
            });

        var handler = new RefreshAccessTokenCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new RefreshAccessTokenCommand(userId, refreshToken);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("user@example.com");
    }

    [Fact]
    public async Task Handle_RefreshAccessTokenCommand_WithInvalidToken_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string invalidRefreshToken = "invalid_token";

        var user = new User(Email.Create("user@example.com"), null);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new RefreshAccessTokenCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new RefreshAccessTokenCommand(userId, invalidRefreshToken);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<BusinessLogicException>();
    }

    [Fact]
    public async Task Handle_RefreshAccessTokenCommand_WithExpiredToken_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string expiredToken = "expired_token";

        var user = new User(Email.Create("user@example.com"), null);
        // Add an expired token (negative expiration minutes creates a past date)
        user.AddRefreshToken(expiredToken, -1);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new RefreshAccessTokenCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new RefreshAccessTokenCommand(userId, expiredToken);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<BusinessLogicException>();
    }

    #endregion

    #region Change Password Command Handler Tests

    [Fact]
    public async Task Handle_ChangePasswordCommand_WithValidCurrentPassword_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string currentPassword = "OldPassword123!";
        const string newPassword = "NewPassword456!";

        var user = new User(
            Email.Create("user@example.com"),
            PasswordHash.Create(currentPassword));

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new ChangePasswordCommandHandler(
            _userRepositoryMock.Object);

        var command = new ChangePasswordCommand(userId, currentPassword, newPassword);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.VerifyPassword(newPassword).Should().BeTrue();
        user.VerifyPassword(currentPassword).Should().BeFalse();

        _userRepositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ChangePasswordCommand_WithWrongCurrentPassword_ShouldThrowBusinessLogicException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string currentPassword = "CorrectPassword123!";
        const string wrongPassword = "WrongPassword123!";
        const string newPassword = "NewPassword456!";

        var user = new User(
            Email.Create("user@example.com"),
            PasswordHash.Create(currentPassword));

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var handler = new ChangePasswordCommandHandler(
            _userRepositoryMock.Object);

        var command = new ChangePasswordCommand(userId, wrongPassword, newPassword);

        // Act & Assert
        var action = async () => await handler.Handle(command, CancellationToken.None);
        await action.Should().ThrowAsync<BusinessLogicException>();
    }

    #endregion

    #region OAuth Login Command Handler Tests

    [Fact]
    public async Task Handle_OAuthLoginCommand_WithNewUser_ShouldCreateUser()
    {
        // Arrange
        const string provider = "google";
        const string providerUserId = "google_123456";
        const string email = "oauth@example.com";
        const string fullName = "OAuth User";

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _userRepositoryMock
            .Setup(r => r.GetByOAuthProviderAsync(provider, providerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(It.IsAny<Guid>(), email, It.IsAny<string>()))
            .Returns("access_token");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpirationMinutes())
            .Returns(10080);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpirationMinutes())
            .Returns(15);

        _mapperMock
            .Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns((User u) =>
            {
                return new UserDto
                {
                    Id = u.Id,
                    Email = u.Email.Value,
                    FullName = GetFullName(u),
                    IsEmailVerified = u.IsEmailConfirmed,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt,
                    OAuthLinks = u.OAuthProviders
                        .Select(op => new OAuthLinkDto
                        {
                            Provider = op.Provider,
                            LinkedAt = op.LinkedAt
                        })
                        .ToList()
                };
            });

        var handler = new OAuthLoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new OAuthLoginCommand(provider, providerUserId, email, fullName);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(email);
        result.AccessToken.Should().NotBeNullOrEmpty();

        _userRepositoryMock.Verify(
            r => r.AddAsync(It.IsAny<User>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_OAuthLoginCommand_WithExistingUser_ShouldReturnExistingUser()
    {
        // Arrange
        const string provider = "google";
        const string providerUserId = "google_123456";
        const string email = "existing@example.com";

        var existingUser = User.CreateFromOAuth(provider, providerUserId, Email.Create(email), "Existing User");

        _userRepositoryMock
            .Setup(r => r.GetByOAuthProviderAsync(provider, providerUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _jwtTokenServiceMock
            .Setup(j => j.GenerateAccessToken(existingUser.Id, email, It.IsAny<string>()))
            .Returns("access_token");

        _jwtTokenServiceMock
            .Setup(j => j.GenerateRefreshToken())
            .Returns("refresh_token");

        _jwtTokenServiceMock
            .Setup(j => j.GetRefreshTokenExpirationMinutes())
            .Returns(10080);

        _jwtTokenServiceMock
            .Setup(j => j.GetAccessTokenExpirationMinutes())
            .Returns(15);

        _mapperMock
            .Setup(m => m.Map<UserDto>(It.IsAny<User>()))
            .Returns((User u) =>
            {
                return new UserDto
                {
                    Id = u.Id,
                    Email = u.Email.Value,
                    FullName = GetFullName(u),
                    IsEmailVerified = u.IsEmailConfirmed,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt,
                    LastLoginAt = u.LastLoginAt,
                    OAuthLinks = u.OAuthProviders
                        .Select(op => new OAuthLinkDto
                        {
                            Provider = op.Provider,
                            LinkedAt = op.LinkedAt
                        })
                        .ToList()
                };
            });

        var handler = new OAuthLoginCommandHandler(
            _userRepositoryMock.Object,
            _jwtTokenServiceMock.Object,
            _mapperMock.Object);

        var command = new OAuthLoginCommand(provider, providerUserId, email, "Existing User");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.User.Email.Should().Be(email);
    }

    #endregion
}
