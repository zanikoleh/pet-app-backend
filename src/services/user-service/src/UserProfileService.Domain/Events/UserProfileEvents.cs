using SharedKernel.Domain;

namespace UserProfileService.Domain.Events;

/// <summary>
/// Event raised when a user profile is created.
/// </summary>
public sealed record UserProfileCreatedEvent(Guid UserProfileId, Guid UserId, string Email) : DomainEvent;

/// <summary>
/// Event raised when a user profile is updated.
/// </summary>
public sealed record UserProfileUpdatedEvent(Guid UserProfileId, Guid UserId) : DomainEvent;

/// <summary>
/// Event raised when user profile is deactivated.
/// </summary>
public sealed record UserProfileDeactivatedEvent(Guid UserProfileId, Guid UserId) : DomainEvent;

/// <summary>
/// Event raised when user profile is reactivated.
/// </summary>
public sealed record UserProfileActivatedEvent(Guid UserProfileId, Guid UserId) : DomainEvent;

/// <summary>
/// Event raised when notification preferences are updated.
/// </summary>
public sealed record NotificationPreferencesUpdatedEvent(
    Guid UserProfileId,
    Guid UserId,
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    bool ReceivePromotions,
    bool ReceiveNewsletter) : DomainEvent;
