using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Commands;

/// <summary>
/// Command to register a new user with email and password.
/// </summary>
public sealed record RegisterCommand(
    string Email,
    string Password,
    string? FullName) : IRequest<AuthenticationResponseDto>;

/// <summary>
/// Command to login a user with email and password.
/// </summary>
public sealed record LoginCommand(
    string Email,
    string Password) : IRequest<AuthenticationResponseDto>;

/// <summary>
/// Command to login or register a user via OAuth provider.
/// </summary>
public sealed record OAuthLoginCommand(
    string Provider,
    string ProviderUserId,
    string Email,
    string? FullName = null,
    string? Avatar = null) : IRequest<AuthenticationResponseDto>;

/// <summary>
/// Command to refresh access token using refresh token.
/// </summary>
public sealed record RefreshAccessTokenCommand(
    Guid UserId,
    string RefreshToken) : IRequest<AuthenticationResponseDto>;

/// <summary>
/// Command to change user password.
/// </summary>
public sealed record ChangePasswordCommand(
    Guid UserId,
    string CurrentPassword,
    string NewPassword) : IRequest<Unit>;

/// <summary>
/// Command to update user profile.
/// </summary>
public sealed record UpdateProfileCommand(
    Guid UserId,
    string? FullName,
    string? Avatar) : IRequest<UserDto>;

/// <summary>
/// Command to verify user email.
/// </summary>
public sealed record VerifyEmailCommand(
    Guid UserId) : IRequest<Unit>;

/// <summary>
/// Command to link OAuth provider to existing account.
/// </summary>
public sealed record LinkOAuthProviderCommand(
    Guid UserId,
    string Provider,
    string ProviderUserId,
    string ProviderEmail) : IRequest<UserDto>;

/// <summary>
/// Command to unlink OAuth provider from account.
/// </summary>
public sealed record UnlinkOAuthProviderCommand(
    Guid UserId,
    string Provider) : IRequest<UserDto>;

/// <summary>
/// Command to deactivate user account.
/// </summary>
public sealed record DeactivateAccountCommand(
    Guid UserId) : IRequest<Unit>;

/// <summary>
/// Command to logout (revoke refresh tokens).
/// </summary>
public sealed record LogoutCommand(
    Guid UserId) : IRequest<Unit>;
