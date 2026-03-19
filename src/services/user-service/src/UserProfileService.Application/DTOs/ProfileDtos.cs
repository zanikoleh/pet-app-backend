namespace UserProfileService.Application.DTOs;

/// <summary>
/// DTO for user profile.
/// </summary>
public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public NotificationPreferencesDto Preferences { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// DTO for notification preferences.
/// </summary>
public class NotificationPreferencesDto
{
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "UTC";
    public bool NotificationsEnabled { get; set; } = true;
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool ReceivePromotions { get; set; } = true;
    public bool ReceiveNewsletter { get; set; } = true;
}

/// <summary>
/// Request to update profile.
/// </summary>
public class UpdateProfileRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Bio { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

/// <summary>
/// Request to update notification preferences.
/// </summary>
public class UpdateNotificationPreferencesRequest
{
    public bool EmailNotifications { get; set; } = true;
    public bool PushNotifications { get; set; } = true;
    public bool SmsNotifications { get; set; } = false;
    public bool ReceivePromotions { get; set; } = true;
    public bool ReceiveNewsletter { get; set; } = true;
}

/// <summary>
/// Request to update language and timezone.
/// </summary>
public class UpdateLanguageRequest
{
    public string Language { get; set; } = "en";
    public string Timezone { get; set; } = "UTC";
}
