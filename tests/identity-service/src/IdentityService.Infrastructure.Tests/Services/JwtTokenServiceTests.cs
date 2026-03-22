using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;
using FluentAssertions;
using IdentityService.Infrastructure.Services;

namespace IdentityService.Infrastructure.Tests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;
    private const string SecretKey = "this_is_a_very_long_secret_key_that_is_at_least_32_characters";
    private const string Issuer = "test-issuer";
    private const string Audience = "test-audience";

    public JwtTokenServiceTests()
    {
        _jwtTokenService = new JwtTokenService(
            SecretKey,
            Issuer,
            Audience,
            accessTokenExpirationMinutes: 15,
            refreshTokenExpirationMinutes: 10080); // 7 days
    }

    #region Constructor Validation Tests

    [Fact]
    public void Constructor_WithNullSecretKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new JwtTokenService(null!, Issuer, Audience);
        action.Should().Throw<ArgumentException>().WithMessage("*Secret key must be at least 32 characters*");
    }

    [Fact]
    public void Constructor_WithShortSecretKey_ShouldThrowArgumentException()
    {
        // Act & Assert
        var action = () => new JwtTokenService("short", Issuer, Audience);
        action.Should().Throw<ArgumentException>().WithMessage("*Secret key must be at least 32 characters*");
    }

    [Fact]
    public void Constructor_WithNullIssuer_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new JwtTokenService(SecretKey, null!, Audience);
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullAudience_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        var action = () => new JwtTokenService(SecretKey, Issuer, null!);
        action.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Access Token Generation Tests

    [Fact]
    public void GenerateAccessToken_WithValidParameters_ShouldReturnValidToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "user@example.com";
        const string role = "User";

        // Act
        var token = _jwtTokenService.GenerateAccessToken(userId, email, role);

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Should().Contain(".");
        token.Split('.').Should().HaveCount(3); // JWT has three parts
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainCorrectClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string role = "Admin";

        // Act
        var token = _jwtTokenService.GenerateAccessToken(userId, email, role);
        var claims = ExtractClaimsFromToken(token);

        // Assert
        claims.Should().ContainKey(ClaimTypes.NameIdentifier);
        claims[ClaimTypes.NameIdentifier].Should().Be(userId.ToString());
        claims.Should().ContainKey(ClaimTypes.Email);
        claims[ClaimTypes.Email].Should().Be(email);
        claims.Should().ContainKey(ClaimTypes.Role);
        claims[ClaimTypes.Role].Should().Be(role);
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainIssuedAtClaim()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "test@example.com";
        const string role = "User";

        // Act
        var token = _jwtTokenService.GenerateAccessToken(userId, email, role);
        var claims = ExtractClaimsFromToken(token);

        // Assert
        claims.Should().ContainKey("iat");
        var iatValue = long.Parse(claims["iat"]);
        iatValue.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GenerateAccessToken_WithDifferentUsers_ShouldGenerateDifferentTokens()
    {
        // Arrange
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();

        // Act
        var token1 = _jwtTokenService.GenerateAccessToken(userId1, "user1@example.com", "User");
        var token2 = _jwtTokenService.GenerateAccessToken(userId2, "user2@example.com", "User");

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveExpiration()
    {
        // Arrange
        var userId = Guid.NewGuid();
        
        // Act
        var token = _jwtTokenService.GenerateAccessToken(userId, "user@example.com", "User");
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        var expirationMinutes = (jwtToken.ValidTo - DateTime.UtcNow).TotalMinutes;
        expirationMinutes.Should().BeGreaterThan(14).And.BeLessThan(16);
    }

    #endregion

    #region Refresh Token Generation Tests

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnRandomTokens()
    {
        // Act
        var token1 = _jwtTokenService.GenerateRefreshToken();
        var token2 = _jwtTokenService.GenerateRefreshToken();
        var token3 = _jwtTokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
        token2.Should().NotBe(token3);
        token1.Should().NotBe(token3);
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnReasonableLengthToken()
    {
        // Act
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        // Assert
        refreshToken.Length.Should().BeGreaterThan(20);
        refreshToken.Length.Should().BeLessThan(500);
    }

    #endregion

    #region Token Expiration Configuration Tests

    [Fact]
    public void GetAccessTokenExpirationMinutes_ShouldReturnConfiguredValue()
    {
        // Act
        var expirationMinutes = _jwtTokenService.GetAccessTokenExpirationMinutes();

        // Assert
        expirationMinutes.Should().Be(15);
    }

    [Fact]
    public void GetRefreshTokenExpirationMinutes_ShouldReturnConfiguredValue()
    {
        // Act
        var expirationMinutes = _jwtTokenService.GetRefreshTokenExpirationMinutes();

        // Assert
        expirationMinutes.Should().Be(10080); // 7 days
    }

    [Fact]
    public void Constructor_WithCustomExpirationTimes_ShouldUseCustomValues()
    {
        // Arrange
        var customAccessExpiration = 30;
        var customRefreshExpiration = 20160; // 14 days

        var service = new JwtTokenService(
            SecretKey,
            Issuer,
            Audience,
            accessTokenExpirationMinutes: customAccessExpiration,
            refreshTokenExpirationMinutes: customRefreshExpiration);

        // Act
        var accessExp = service.GetAccessTokenExpirationMinutes();
        var refreshExp = service.GetRefreshTokenExpirationMinutes();

        // Assert
        accessExp.Should().Be(customAccessExpiration);
        refreshExp.Should().Be(customRefreshExpiration);
    }

    #endregion

    #region Token Validity Tests

    [Fact]
    public void GenerateAccessToken_ShouldCreateValidJwtToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var handler = new JwtSecurityTokenHandler();

        // Act
        var token = _jwtTokenService.GenerateAccessToken(userId, "user@example.com", "User");
        
        // Assert - should not throw
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Should().NotBeNull();
        jwtToken.Issuer.Should().Be(Issuer);
        jwtToken.Audiences.Should().Contain(Audience);
    }

    [Fact]
    public void GenerateAccessToken_WithSpecialCharactersInEmail_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "user+test@example.com";

        // Act
        var token = _jwtTokenService.GenerateAccessToken(userId, email, "User");
        var claims = ExtractClaimsFromToken(token);

        // Assert
        claims[ClaimTypes.Email].Should().Be(email);
    }

    [Fact]
    public void GenerateAccessToken_WithLongEmail_ShouldSucceed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        const string email = "verylongemailaddresstotesthandlingofextendedaddresses@subdomain.example.com";

        // Act
        var token = _jwtTokenService.GenerateAccessToken(userId, email, "User");
        var claims = ExtractClaimsFromToken(token);

        // Assert
        claims[ClaimTypes.Email].Should().Be(email);
    }

    #endregion

    #region Helper Methods

    private Dictionary<string, string> ExtractClaimsFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        return jwtToken.Claims
            .DistinctBy(c => c.Type)
            .ToDictionary(c => c.Type, c => c.Value);
    }

    #endregion
}
