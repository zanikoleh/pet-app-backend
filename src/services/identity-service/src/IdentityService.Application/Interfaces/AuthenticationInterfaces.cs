using SharedKernel.Infrastructure;

namespace IdentityService.Application.Interfaces;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for a user.
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, string role);

    /// <summary>
    /// Generates a refresh token string.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates and gets claims from a JWT token.
    /// </summary>
    (bool IsValid, Guid UserId, string Email) ValidateAccessToken(string token);

    /// <summary>
    /// Gets access token expiration time.
    /// </summary>
    int GetAccessTokenExpirationMinutes();

    /// <summary>
    /// Gets refresh token expiration time.
    /// </summary>
    int GetRefreshTokenExpirationMinutes();
}

/// <summary>
/// Service for OAuth provider integration.
/// </summary>
public interface IOAuthProviderService
{
    /// <summary>
    /// Gets OAuth provider user info from authorization code.
    /// </summary>
    Task<(string ProviderUserId, string Email, string? Name, string? Avatar)> GetUserInfoAsync(
        string provider,
        string code,
        string? idToken = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for User aggregate.
/// </summary>
public interface IUserRepository : IRepository<Domain.Aggregates.User, Guid>
{
    /// <summary>
    /// Finds user by email address.
    /// </summary>
    Task<Domain.Aggregates.User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds user by OAuth provider.
    /// </summary>
    Task<Domain.Aggregates.User?> GetByOAuthProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if email already exists.
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
