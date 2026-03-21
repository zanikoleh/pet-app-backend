using SharedKernel;

namespace UserProfileService.Domain.Events;

/// <summary>
/// Event raised when a user profile is created.
/// </summary>
public class UserProfileCreatedEvent : DomainEvent
{
    public Guid UserProfileId { get; }
    public Guid UserId { get; }
    public string Email { get; }

    public UserProfileCreatedEvent(Guid userProfileId, Guid userId, string email)
    {
        UserProfileId = userProfileId;
        UserId = userId;
        Email = email;
    }
}

/// <summary>
/// Event raised when a user profile is updated.
/// </summary>
public class UserProfileUpdatedEvent : DomainEvent
{
    public Guid UserProfileId { get; }
    public Guid UserId { get; }

    public UserProfileUpdatedEvent(Guid userProfileId, Guid userId)
    {
        UserProfileId = userProfileId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when user profile is deactivated.
/// </summary>
public class UserProfileDeactivatedEvent : DomainEvent
{
    public Guid UserProfileId { get; }
    public Guid UserId { get; }

    public UserProfileDeactivatedEvent(Guid userProfileId, Guid userId)
    {
        UserProfileId = userProfileId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when user profile is reactivated.
/// </summary>
public class UserProfileActivatedEvent : DomainEvent
{
    public Guid UserProfileId { get; }
    public Guid UserId { get; }

    public UserProfileActivatedEvent(Guid userProfileId, Guid userId)
    {
        UserProfileId = userProfileId;
        UserId = userId;
    }
}

/// <summary>
/// Event raised when notification preferences are updated.
/// </summary>
public class NotificationPreferencesUpdatedEvent : DomainEvent
{
    public Guid UserProfileId { get; }
    public Guid UserId { get; }
    public bool EmailNotifications { get; }
    public bool PushNotifications { get; }
    public bool SmsNotifications { get; }
    public bool ReceivePromotions { get; }
    public bool ReceiveNewsletter { get; }

    public NotificationPreferencesUpdatedEvent(
        Guid userProfileId,
        Guid userId,
        bool emailNotifications,
        bool pushNotifications,
        bool smsNotifications,
        bool receivePromotions,
        bool receiveNewsletter)
    {
        UserProfileId = userProfileId;
        UserId = userId;
        EmailNotifications = emailNotifications;
        PushNotifications = pushNotifications;
        SmsNotifications = smsNotifications;
        ReceivePromotions = receivePromotions;
        ReceiveNewsletter = receiveNewsletter;
    }
}
