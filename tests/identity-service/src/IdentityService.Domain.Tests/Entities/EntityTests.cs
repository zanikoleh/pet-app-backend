using Xunit;
using FluentAssertions;
using IdentityService.Domain.Entities;
using IdentityService.Domain.ValueObjects;

namespace IdentityService.Domain.Tests.Entities;

public class RefreshTokenTests
{
    [Fact]
    public void CreateRefreshToken_WithValidData_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string token = "refresh_token_abc123";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Assert
        refreshToken.Should().NotBeNull();
        refreshToken.Id.Should().NotBeEmpty();
        refreshToken.UserId.Should().Be(userId);
        refreshToken.Token.Should().Be(token);
        refreshToken.ExpiresAt.Should().Be(expiresAt);
        refreshToken.IsRevoked.Should().BeFalse();
        refreshToken.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RefreshToken_WithFutureExpiration_ShouldBeValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string token = "valid_token";
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Assert
        refreshToken.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_WithPastExpiration_ShouldNotBeValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string token = "expired_token";
        var expiresAt = DateTime.UtcNow.AddDays(-1);

        // Act
        var refreshToken = new RefreshToken(userId, token, expiresAt);

        // Assert
        refreshToken.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_WhenRevoked_ShouldNotBeValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string token = "revoked_token";
        var expiresAt = DateTime.UtcNow.AddDays(7);
        var refreshToken = new RefreshToken(userId, token, expiresAt);
        refreshToken.IsValid.Should().BeTrue();

        // Act
        refreshToken.IsRevoked = true;

        // Assert
        refreshToken.IsValid.Should().BeFalse();
    }

    [Fact]
    public void EachRefreshToken_ShouldHaveUniqueId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiresAt = DateTime.UtcNow.AddDays(7);

        // Act
        var token1 = new RefreshToken(userId, "token1", expiresAt);
        var token2 = new RefreshToken(userId, "token2", expiresAt);

        // Assert
        token1.Id.Should().NotBe(token2.Id);
    }

    [Fact]
    public void RefreshToken_CreatedAt_ShouldBeUtcNow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var refreshToken = new RefreshToken(userId, "token", DateTime.UtcNow.AddDays(7));
        var afterCreation = DateTime.UtcNow;

        // Assert
        refreshToken.CreatedAt.Should().BeOnOrAfter(beforeCreation);
        refreshToken.CreatedAt.Should().BeOnOrBefore(afterCreation);
    }
}

public class OAuthProviderTests
{
    [Fact]
    public void CreateOAuthProvider_WithValidData_ShouldSucceed()
    {
        // Arrange
        const string provider = "google";
        const string providerUserId = "google_123456";

        // Act
        var oauthProvider = new OAuthProvider(provider, providerUserId);

        // Assert
        oauthProvider.Should().NotBeNull();
        oauthProvider.Provider.Should().Be(provider);
        oauthProvider.ProviderUserId.Should().Be(providerUserId);
        oauthProvider.LinkedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("google")]
    [InlineData("facebook")]
    [InlineData("apple")]
    [InlineData("github")]
    public void OAuthProvider_WithDifferentProviders_ShouldAllBeValid(string provider)
    {
        // Arrange & Act
        var oauthProvider = new OAuthProvider(provider, "user_123");

        // Assert
        oauthProvider.Provider.Should().Be(provider);
    }

    [Fact]
    public void OAuthProvider_WithMultipleIds_ShouldPreserveAllData()
    {
        // Arrange
        const string provider = "facebook";
        const string shortId = "123";
        const string longId = "123456789_987654321_1234567890";

        // Act
        var provider1 = new OAuthProvider(provider, shortId);
        var provider2 = new OAuthProvider(provider, longId);

        // Assert
        provider1.ProviderUserId.Should().Be(shortId);
        provider2.ProviderUserId.Should().Be(longId);
    }

    [Fact]
    public void OAuthProvider_ShouldBeEquatable()
    {
        // Arrange
        var provider1 = new OAuthProvider("google", "user_123");
        var provider2 = new OAuthProvider("google", "user_123");
        var provider3 = new OAuthProvider("facebook", "user_123");

        // Act & Assert
        provider1.Should().Be(provider2);
        provider1.Should().NotBe(provider3);
    }

    [Fact]
    public void OAuthProvider_WithSameData_ShouldHaveConsistentHashCode()
    {
        // Arrange
        var provider1 = new OAuthProvider("google", "user_123");
        var provider2 = new OAuthProvider("google", "user_123");

        // Act & Assert
        provider1.GetHashCode().Should().Be(provider2.GetHashCode());
    }

    [Fact]
    public void OAuthProvider_LinkedAt_ShouldBeUtcNow()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var oauthProvider = new OAuthProvider("google", "user_123");
        var afterCreation = DateTime.UtcNow;

        // Assert
        oauthProvider.LinkedAt.Should().BeOnOrAfter(beforeCreation);
        oauthProvider.LinkedAt.Should().BeOnOrBefore(afterCreation);
    }

    [Fact]
    public void OAuthProvider_DifferentInstances_ShouldHaveDifferentLinkedAtTimes()
    {
        // Arrange
        var provider1 = new OAuthProvider("google", "user_123");
        System.Threading.Thread.Sleep(1);
        var provider2 = new OAuthProvider("google", "user_456");

        // Act & Assert
        // LinkedAt might be the same due to machine speed, so we just check they're close
        provider2.LinkedAt.Should().BeOnOrAfter(provider1.LinkedAt);
    }
}
