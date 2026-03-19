using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using IdentityService.Application.Commands;
using IdentityService.Application.Queries;
using IdentityService.Application.DTOs;

namespace IdentityService.Api.Controllers;

/// <summary>
/// Controller for authentication operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Register a new user with email and password.
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthenticationResponseDto>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RegisterCommand(request.Email, request.Password, request.FullName);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Login with email and password.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponseDto>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Login or register with OAuth provider.
    /// </summary>
    [HttpPost("oauth-login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthenticationResponseDto>> OAuthLogin(
        [FromBody] OAuthLoginRequest request,
        CancellationToken cancellationToken = default)
    {
        // In production, you would validate the code/idToken with the OAuth provider first
        // For now, this assumes the frontend has already validated with the provider

        var command = new OAuthLoginCommand(
            request.Provider,
            request.Provider.ToLowerInvariant(), // Placeholder; normally extracted from token validation
            $"user-{Guid.NewGuid()}@example.com", // Placeholder; normally from OAuth provider
            null,
            null);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Refresh access token using refresh token.
    /// </summary>
    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthenticationResponseDto>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        // Extract userId from the request (in real implementation, from token claims)
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new RefreshAccessTokenCommand(parsedUserId, request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Change user password.
    /// </summary>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new ChangePasswordCommand(parsedUserId, request.CurrentPassword, request.NewPassword);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Get current user profile.
    /// </summary>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetProfile(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var query = new GetUserProfileQuery(parsedUserId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update user profile.
    /// </summary>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new UpdateProfileCommand(parsedUserId, request.FullName, request.Avatar);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Link OAuth provider to account.
    /// </summary>
    [HttpPost("link-oauth")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> LinkOAuthProvider(
        [FromBody] LinkOAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new LinkOAuthProviderCommand(
            parsedUserId,
            request.Provider,
            request.ProviderUserId,
            request.ProviderEmail);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Unlink OAuth provider from account.
    /// </summary>
    [HttpPost("unlink-oauth")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> UnlinkOAuthProvider(
        [FromBody] UnlinkOAuthRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new UnlinkOAuthProviderCommand(parsedUserId, request.Provider);
        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Logout (revoke all refresh tokens).
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Logout(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new LogoutCommand(parsedUserId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deactivate user account.
    /// </summary>
    [HttpPost("deactivate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> DeactivateAccount(CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new DeactivateAccountCommand(parsedUserId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Check if email exists.
    /// </summary>
    [HttpPost("check-email")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmailCheckResponse>> CheckEmail(
        [FromBody] EmailCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = new EmailExistsQuery(request.Email);
        var exists = await _mediator.Send(query, cancellationToken);
        return Ok(new EmailCheckResponse { Exists = exists });
    }
}

/// <summary>
/// Request for linking OAuth provider.
/// </summary>
public class LinkOAuthRequest
{
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string ProviderEmail { get; set; } = string.Empty;
}

/// <summary>
/// Request for unlinking OAuth provider.
/// </summary>
public class UnlinkOAuthRequest
{
    public string Provider { get; set; } = string.Empty;
}

/// <summary>
/// Request for checking email.
/// </summary>
public class EmailCheckRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Response for email check.
/// </summary>
public class EmailCheckResponse
{
    public bool Exists { get; set; }
}
