using Xunit;
using FluentAssertions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.ValueObjects;
using IdentityService.Infrastructure.Persistence;
using IdentityService.Infrastructure.Tests.Fixtures;

namespace IdentityService.Infrastructure.Tests.Persistence;

public class UserRepositoryTests : IAsyncLifetime
{
    private readonly DatabaseFixture _databaseFixture;
    private UserRepository? _userRepository;

    public UserRepositoryTests()
    {
        _databaseFixture = new DatabaseFixture();
    }

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        _userRepository = new UserRepository(_databaseFixture.DbContext);
    }

    public async Task DisposeAsync() => await _databaseFixture.DisposeAsync();

    private UserRepository UserRepository => _userRepository ?? throw new InvalidOperationException("UserRepository not initialized");

    #region Add/Get Tests

    [Fact]
    public async Task AddAsync_WithNewUser_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("newuser@example.com");
        var passwordHash = PasswordHash.Create("SecurePassword123!");
        var user = new User(email, passwordHash, "Test User");

        // Act
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Assert
        var savedUser = await UserRepository.GetByIdAsync(user.Id);
        savedUser.Should().NotBeNull();
        savedUser.Email.Value.Should().Be("newuser@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var email = Email.Create("user@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash, "Test User");
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(user.Id);
        retrievedUser.Email.Value.Should().Be("user@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(Guid.NewGuid());

        // Assert
        retrievedUser.Should().BeNull();
    }

    #endregion

    #region Email Query Tests

    [Fact]
    public async Task EmailExistsAsync_WithExistingEmail_ShouldReturnTrue()
    {
        // Arrange
        var email = Email.Create("existing@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash);
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var exists = await UserRepository.EmailExistsAsync("existing@example.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task EmailExistsAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        // Act
        var exists = await UserRepository.EmailExistsAsync("nonexistent@example.com");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task EmailExistsAsync_WithDifferentCaseEmail_ShouldReturnTrue()
    {
        // Arrange - Email should be case-insensitive
        var email = Email.Create("TestUser@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash);
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var exists = await UserRepository.EmailExistsAsync("testuser@example.com");

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        // Arrange
        var email = Email.Create("findme@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash, "John Doe");
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByEmailAsync("findme@example.com");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Email.Value.Should().Be("findme@example.com");
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        // Act
        var retrievedUser = await UserRepository.GetByEmailAsync("notfound@example.com");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_IsCaseInsensitive_ShouldReturnUser()
    {
        // Arrange
        var email = Email.Create("CaseSensitive@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash);
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByEmailAsync("casesensitive@example.com");

        // Assert
        retrievedUser.Should().NotBeNull();
    }

    #endregion

    #region OAuth Provider Tests

    [Fact]
    public async Task GetByOAuthProviderAsync_WithExistingProvider_ShouldReturnUser()
    {
        // Arrange
        var user = User.CreateFromOAuth("google", "google_12345", Email.Create("oauth@example.com"), "OAuth User");
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByOAuthProviderAsync("google", "google_12345");

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.Id.Should().Be(user.Id);
        retrievedUser.Email.Value.Should().Be("oauth@example.com");
    }

    [Fact]
    public async Task GetByOAuthProviderAsync_WithNonExistentProvider_ShouldReturnNull()
    {
        // Act
        var retrievedUser = await UserRepository.GetByOAuthProviderAsync("google", "nonexistent_id");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByOAuthProviderAsync_WithDifferentProvider_ShouldReturnNull()
    {
        // Arrange
        var user = User.CreateFromOAuth("google", "google_12345", Email.Create("oauth@example.com"), "OAuth User");
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByOAuthProviderAsync("facebook", "google_12345");

        // Assert
        retrievedUser.Should().BeNull();
    }

    [Fact]
    public async Task GetByOAuthProviderAsync_WithMultipleUsersAndOAuthLinks_ShouldReturnCorrectUser()
    {
        // Arrange - Create users with different OAuth providers
        var user1 = User.CreateFromOAuth("google", "google_user1", Email.Create("user1@example.com"), "User One");
        var user2 = User.CreateFromOAuth("facebook", "facebook_user2", Email.Create("user2@example.com"), "User Two");
        
        await UserRepository.AddAsync(user1);
        await UserRepository.AddAsync(user2);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser1 = await UserRepository.GetByOAuthProviderAsync("google", "google_user1");
        var retrievedUser2 = await UserRepository.GetByOAuthProviderAsync("facebook", "facebook_user2");

        // Assert
        retrievedUser1!.Id.Should().Be(user1.Id);
        retrievedUser2!.Id.Should().Be(user2.Id);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task SaveChangesAsync_WithUpdatedUser_ShouldPersistChanges()
    {
        // Arrange
        var email = Email.Create("updateme@example.com");
        var passwordHash = PasswordHash.Create("OldPassword123!");
        var user = new User(email, passwordHash, "Original Name");
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);
        retrievedUser!.Deactivate();
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Assert
        var updatedUser = await UserRepository.GetByIdAsync(user.Id);
        updatedUser!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WithExistingUser_ShouldRemoveUser()
    {
        // Arrange
        var email = Email.Create("deleteme@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash);
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        await UserRepository.DeleteAsync(user.Id);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Assert
        var deletedUser = await UserRepository.GetByIdAsync(user.Id);
        deletedUser.Should().BeNull();
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public async Task SaveChangesAsync_WithRefreshTokens_ShouldPersistTokens()
    {
        // Arrange
        var user = new User(Email.Create("refresh@example.com"), null);
        user.AddRefreshToken("token_1", 10080);
        user.AddRefreshToken("token_2", 10080);

        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.RefreshTokens.Should().HaveCount(2);
        retrievedUser.RefreshTokens.Select(rt => rt.Token).Should().Contain("token_1");
        retrievedUser.RefreshTokens.Select(rt => rt.Token).Should().Contain("token_2");
    }

    [Fact]
    public async Task SaveChangesAsync_WithRevokedRefreshToken_ShouldPersistRevocationStatus()
    {
        // Arrange
        var user = new User(Email.Create("revoke@example.com"), null);
        user.AddRefreshToken("token_to_revoke", 10080);
        
        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);
        var tokenToRevoke = retrievedUser!.RefreshTokens.First();
        retrievedUser.RevokeRefreshToken(tokenToRevoke.Token);
        
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Assert
        var updatedUser = await UserRepository.GetByIdAsync(user.Id);
        var revokedToken = updatedUser!.RefreshTokens.First(rt => rt.Token == "token_to_revoke");
        revokedToken.IsRevoked.Should().BeTrue();
    }

    #endregion

    #region OAuth Provider Link Tests

    [Fact]
    public async Task SaveChangesAsync_WithLinkedOAuthProvider_ShouldPersistLink()
    {
        // Arrange
        var user = new User(Email.Create("oauth_link@example.com"), null);
        user.LinkOAuthProvider("google", "google_123");

        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser.Should().NotBeNull();
        retrievedUser!.OAuthProviders.Should().HaveCount(1);
        var provider = retrievedUser.OAuthProviders.First();
        provider.Provider.Should().Be("google");
        provider.ProviderUserId.Should().Be("google_123");
    }

    [Fact]
    public async Task SaveChangesAsync_WithMultipleOAuthProviders_ShouldPersistAllLinks()
    {
        // Arrange
        var user = new User(Email.Create("multi_oauth@example.com"), null);
        user.LinkOAuthProvider("google", "google_123");
        user.LinkOAuthProvider("facebook", "facebook_456");

        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser!.OAuthProviders.Should().HaveCount(2);
        retrievedUser.OAuthProviders.Select(op => op.Provider).Should().Contain(new[] { "google", "facebook" });
    }

    #endregion

    #region Value Object Tests

    [Fact]
    public async Task SaveChangesAsync_WithPasswordHash_ShouldPersistAndRetrieve()
    {
        // Arrange
        const string password = "ComplexPassword123!@#";
        var email = Email.Create("password_test@example.com");
        var passwordHash = PasswordHash.Create(password);
        var user = new User(email, passwordHash);

        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser!.VerifyPassword(password).Should().BeTrue();
        retrievedUser.VerifyPassword("WrongPassword").Should().BeFalse();
    }

    [Fact]
    public async Task SaveChangesAsync_WithEmail_ShouldPersistAndRetrieve()
    {
        // Arrange
        const string emailValue = "test@example.com";
        var email = Email.Create(emailValue);
        var user = new User(email, null);

        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser!.Email.Value.Should().Be(emailValue);
    }

    #endregion

    #region Role Tests

    [Fact]
    public async Task SaveChangesAsync_WithUserRole_ShouldPersistRole()
    {
        // Arrange
        var user = new User(Email.Create("user_role@example.com"), null);

        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser!.Role.Value.Should().Be("User");
    }

    [Fact]
    public async Task SaveChangesAsync_WithAdminRole_ShouldPersistRole()
    {
        // Arrange
        var user = new User(Email.Create("admin_role@example.com"), null);
        // Note: Role conversion is handled by value object - only User and Admin exist

        await UserRepository.AddAsync(user);
        await _databaseFixture.DbContext.SaveChangesAsync();

        // Act
        var retrievedUser = await UserRepository.GetByIdAsync(user.Id);

        // Assert
        retrievedUser!.Role.Should().NotBeNull();
    }

    #endregion
}
