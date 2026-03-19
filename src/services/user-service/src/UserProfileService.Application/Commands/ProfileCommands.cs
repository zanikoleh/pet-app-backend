using MediatR;
using UserProfileService.Application.DTOs;

namespace UserProfileService.Application.Commands;

/// <summary>
/// Command to update user profile.
/// </summary>
public sealed record UpdateProfileCommand(
    Guid UserProfileId,
    string? FirstName,
    string? LastName,
    string? Bio,
    DateTime? DateOfBirth,
    string? PhoneNumber,
    string? Address,
    string? City,
    string? Country,
    string? ProfilePictureUrl) : IRequest<UserProfileDto>;

/// <summary>
/// Command to update notification preferences.
/// </summary>
public sealed record UpdateNotificationPreferencesCommand(
    Guid UserProfileId,
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    bool ReceivePromotions,
    bool ReceiveNewsletter) : IRequest<NotificationPreferencesDto>;

/// <summary>
/// Command to update language and timezone.
/// </summary>
public sealed record UpdateLanguageAndTimezoneCommand(
    Guid UserProfileId,
    string Language,
    string Timezone) : IRequest<Unit>;

/// <summary>
/// Command to deactivate user profile (from Identity Service).
/// </summary>
public sealed record DeactivateProfileCommand(Guid UserProfileId) : IRequest<Unit>;

/// <summary>
/// Command to create user profile from registration event.
/// </summary>
public sealed record CreateProfileFromRegistrationCommand(
    Guid UserId,
    string Email,
    string? FullName,
    string? Avatar) : IRequest<Guid>;
