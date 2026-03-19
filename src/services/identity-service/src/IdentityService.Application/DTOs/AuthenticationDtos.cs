namespace IdentityService.Application.DTOs;

/// <summary>
/// DTO for user profile response.
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public List<OAuthLinkDto> OAuthLinks { get; set; } = new();
}

/// <summary>
/// DTO for OAuth provider link.
/// </summary>
public class OAuthLinkDto
{
    public string Provider { get; set; } = string.Empty;
    public DateTime LinkedAt { get; set; }
}

/// <summary>
/// DTO for authentication response with tokens.
/// </summary>
public class AuthenticationResponseDto
{
    public UserDto User { get; set; } = null!;
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// Request for user registration with email/password.
/// </summary>
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
}

/// <summary>
/// Request for user login with email/password.
/// </summary>
public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// Request for OAuth login/registration.
/// </summary>
public class OAuthLoginRequest
{
    public string Provider { get; set; } = string.Empty; // "google", "facebook", "apple"
    public string Code { get; set; } = string.Empty;
    public string? IdToken { get; set; } // For Apple
}

/// <summary>
/// Request for token refresh.
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Request for password change.
/// </summary>
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Request for updating user profile.
/// </summary>
public class UpdateProfileRequest
{
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
}
