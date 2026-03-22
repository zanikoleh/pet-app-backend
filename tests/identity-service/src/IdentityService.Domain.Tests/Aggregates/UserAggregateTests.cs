using Xunit;
using FluentAssertions;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.ValueObjects;
using IdentityService.Domain.Events;
using SharedKernel;

namespace IdentityService.Domain.Tests.Aggregates;

public class UserAggregateTests
{
    #region User Creation Tests

    [Fact]
    public void CreateUser_WithValidEmailAndPassword_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.Create("SecurePassword123!");
        var fullName = "John Doe";

        // Act
        var user = new User(email, passwordHash, fullName);

        // Assert
        user.Should().NotBeNull();
        user.Email.Value.Should().Be("test@example.com");
        user.FirstName.Should().Be("John");
        user.LastName.Should().Be("Doe");
        user.IsActive.Should().BeTrue();
        user.IsEmailConfirmed.Should().BeFalse();
        user.Role.Should().Be(Role.User);
        user.DomainEvents.Should().ContainSingle(e => e is UserRegisteredEvent);
    }

    [Fact]
    public void CreateUser_WithOAuthProvider_ShouldAutoConfirmEmail()
    {
        // Arrange
        var email = Email.Create("oauth@example.com");

        // Act
        var user = new User(email, null, "OAuth User");

        // Assert
        user.IsEmailConfirmed.Should().BeTrue();
        user.PasswordHash.Should().BeNull();
    }

    [Fact]
    public void CreateUser_RaisesUserRegisteredEvent()
    {
        // Arrange
        var email = Email.Create("newuser@example.com");
        var passwordHash = PasswordHash.Create("Password123!");

        // Act
        var user = new User(email, passwordHash, "Jane Doe");

        // Assert
        var evt = user.DomainEvents.OfType<UserRegisteredEvent>().FirstOrDefault();
        evt.Should().NotBeNull();
        evt!.UserId.Should().Be(user.Id);
        evt.Email.Should().Be("newuser@example.com");
        evt.FirstName.Should().Be("Jane");
        evt.LastName.Should().Be("Doe");
    }

    #endregion

    #region OAuth Provider Tests

    [Fact]
    public void LinkOAuthProvider_WithValidProvider_ShouldSucceed()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash);
        user.ClearDomainEvents();

        // Act
        user.LinkOAuthProvider("google", "google_123456");

        // Assert
        user.OAuthProviders.Should().ContainSingle();
        user.OAuthProviders.First().Provider.Should().Be("google");
        user.OAuthProviders.First().ProviderUserId.Should().Be("google_123456");
        user.DomainEvents.Should().ContainSingle(e => e is OAuthProviderLinkedEvent);
    }

    [Fact]
    public void LinkOAuthProvider_WithDuplicateProvider_ShouldThrowDomainException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        user.LinkOAuthProvider("facebook", "fb_123");

        // Act & Assert
        var action = () => user.LinkOAuthProvider("facebook", "fb_456");
        action.Should().Throw<DomainException>()
            .WithMessage("*already linked*");
    }

    [Fact]
    public void LinkOAuthProvider_MultipleProviders_ShouldAllowMultiple()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);

        // Act
        user.LinkOAuthProvider("google", "google_123");
        user.LinkOAuthProvider("facebook", "fb_123");
        user.LinkOAuthProvider("apple", "apple_123");

        // Assert
        user.OAuthProviders.Should().HaveCount(3);
    }

    [Fact]
    public void UnlinkOAuthProvider_WithExistingProvider_ShouldRemove()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        user.LinkOAuthProvider("google", "google_123");
        user.LinkOAuthProvider("facebook", "fb_123");

        // Act
        user.UnlinkOAuthProvider("google");

        // Assert
        user.OAuthProviders.Should().HaveCount(1);
        user.OAuthProviders.First().Provider.Should().Be("facebook");
    }

    [Fact]
    public void UnlinkOAuthProvider_WithNonExistentProvider_ShouldNotThrow()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        user.LinkOAuthProvider("google", "google_123");

        // Act & Assert
        var action = () => user.UnlinkOAuthProvider("facebook");
        action.Should().NotThrow();
    }

    [Fact]
    public void HasOAuthProvider_WithMatchingProvider_ShouldReturnTrue()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        user.LinkOAuthProvider("google", "google_123");

        // Act
        var result = user.HasOAuthProvider("google", "google_123");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasOAuthProvider_WithoutProvider_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);

        // Act
        var result = user.HasOAuthProvider("google", "google_123");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void CreateFromOAuth_ShouldCreateUserWithProvider()
    {
        // Arrange
        var email = Email.Create("oauth@example.com");
        var avatar = "https://example.com/avatar.png";

        // Act
        var user = User.CreateFromOAuth("google", "google_123", email, "OAuth User", avatar);

        // Assert
        user.Email.Value.Should().Be("oauth@example.com");
        user.FirstName.Should().Be("OAuth");
        user.LastName.Should().Be("User");
        user.Avatar.Should().Be(avatar);
        user.IsEmailConfirmed.Should().BeTrue();
        user.OAuthProviders.Should().HaveCount(1);
        user.OAuthProviders.First().Provider.Should().Be("google");
    }

    #endregion

    #region Password Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        const string password = "SecurePass123!";
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.Create(password);
        var user = new User(email, passwordHash);

        // Act
        var result = user.VerifyPassword(password);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.Create("CorrectPassword123!");
        var user = new User(email, passwordHash);

        // Act
        var result = user.VerifyPassword("WrongPassword123!");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithoutPasswordHash_ShouldReturnFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null); // OAuth user without password

        // Act
        var result = user.VerifyPassword("SomePassword123!");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ChangePassword_WithNewPassword_ShouldUpdate()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var oldPasswordHash = PasswordHash.Create("OldPassword123!");
        var user = new User(email, oldPasswordHash);
        user.ClearDomainEvents();

        const string newPassword = "NewPassword456!";
        var newPasswordHash = PasswordHash.Create(newPassword);

        // Act
        user.ChangePassword(newPasswordHash);

        // Assert
        user.VerifyPassword(newPassword).Should().BeTrue();
        user.VerifyPassword("OldPassword123!").Should().BeFalse();
        user.UpdatedAt.Should().NotBeNull();
        user.DomainEvents.Should().ContainSingle(e => e is UserPasswordChangedEvent);
    }

    [Fact]
    public void ChangePassword_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash);

        // Act & Assert
        var action = () => user.ChangePassword(null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Profile Update Tests

    [Fact]
    public void UpdateProfile_WithNewValues_ShouldUpdate()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null, "Old Name");
        user.ClearDomainEvents();
        var newAvatar = "https://example.com/new-avatar.png";

        // Act
        user.UpdateProfile("Jane", "Smith", newAvatar);

        // Assert
        user.FirstName.Should().Be("Jane");
        user.LastName.Should().Be("Smith");
        user.Avatar.Should().Be(newAvatar);
        user.UpdatedAt.Should().NotBeNull();
        user.DomainEvents.Should().ContainSingle(e => e is UserProfileUpdatedEvent);
    }

    [Fact]
    public void UpdateProfile_WithNullValues_ShouldKeepExisting()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null, "John Doe");
        user.UpdateProfile("Jane", "Smith", "avatar.png");
        user.ClearDomainEvents();

        // Act
        user.UpdateProfile(null, null, null);

        // Assert
        user.FirstName.Should().BeNull();
        user.LastName.Should().BeNull();
        user.Avatar.Should().BeNull();
    }

    #endregion

    #region Email Verification Tests

    [Fact]
    public void VerifyEmail_ShouldMarkEmailAsConfirmed()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var passwordHash = PasswordHash.Create("Password123!");
        var user = new User(email, passwordHash);
        user.IsEmailConfirmed.Should().BeFalse();

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailConfirmed.Should().BeTrue();
        user.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Login Recording Tests

    [Fact]
    public void RecordLogin_ShouldUpdateLastLoginTime()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        var initialLastLogin = user.LastLoginAt;

        // Act
        System.Threading.Thread.Sleep(10);
        user.RecordLogin();

        // Assert
        user.LastLoginAt.Should().BeAfter(initialLastLogin);
        user.UpdatedAt.Should().NotBeNull();
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public void AddRefreshToken_ShouldAddValidToken()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        const string token = "refresh_token_abc123";
        const int expirationMinutes = 10080; // 7 days

        // Act
        user.AddRefreshToken(token, expirationMinutes);

        // Assert
        user.RefreshTokens.Should().ContainSingle();
        user.RefreshTokens.First().Token.Should().Be(token);
        user.RefreshTokens.First().IsValid.Should().BeTrue();
    }

    [Fact]
    public void GetValidRefreshToken_WithValidToken_ShouldReturn()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        const string token = "valid_token_123";
        user.AddRefreshToken(token, 10080);

        // Act
        var result = user.GetValidRefreshToken(token);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token);
    }

    [Fact]
    public void GetValidRefreshToken_WithInvalidToken_ShouldReturnNull()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);

        // Act
        var result = user.GetValidRefreshToken("nonexistent_token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AddRefreshToken_ShouldAllowMultipleTokens()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);

        // Act
        user.AddRefreshToken("token_1", 10080);
        user.AddRefreshToken("token_2", 10080);
        user.AddRefreshToken("token_3", 10080);

        // Assert
        user.RefreshTokens.Should().HaveCount(3);
    }

    #endregion

    #region Account Deactivation Tests

    [Fact]
    public void Deactivate_ShouldSetIsActiveFalse()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var user = new User(email, null);
        user.IsActive.Should().BeTrue();

        // Act
        user.Deactivate();

        // Assert
        user.IsActive.Should().BeFalse();
        user.DomainEvents.Should().ContainSingle(e => e is UserDeactivatedEvent);
    }

    #endregion

    #region User Creation with Full Names Tests

    [Theory]
    [InlineData("John")]
    [InlineData("John Doe")]
    [InlineData("John Michael Doe")]
    public void CreateUser_WithVariousFullNames_ShouldParseCorrectly(string fullName)
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var user = new User(email, null, fullName);

        // Assert
        user.FirstName.Should().NotBeNullOrEmpty();
        user.FirstName.Should().Be(fullName.Split(' ')[0]);
    }

    [Fact]
    public void CreateUser_WithEmptyFullName_ShouldHaveNullNames()
    {
        // Arrange
        var email = Email.Create("test@example.com");

        // Act
        var user = new User(email, null, "");

        // Assert
        user.FirstName.Should().BeNull();
        user.LastName.Should().BeNull();
    }

    #endregion

    #region User Properties Tests

    [Fact]
    public void NewUser_HasCorrectDefaultValues()
    {
        // Arrange
        var email = Email.Create("newuser@example.com");
        var passwordHash = PasswordHash.Create("Password123!");

        // Act
        var user = new User(email, passwordHash, "Test User");

        // Assert
        user.Id.Should().NotBeEmpty();
        user.Email.Value.Should().Be("newuser@example.com");
        user.IsActive.Should().BeTrue();
        user.IsEmailConfirmed.Should().BeFalse();
        user.Role.Should().Be(Role.User);
        user.LastLoginAt.Should().Be(default(DateTime));
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        user.UpdatedAt.Should().BeNull();
    }

    #endregion
}
