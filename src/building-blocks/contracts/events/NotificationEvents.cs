namespace Contracts.Events;

/// <summary>
/// Raised when an email notification should be sent.
/// Published by various services.
/// Subscribed by Notification Service.
/// </summary>
public class SendEmailNotificationEvent : DomainEventNotification
{
    public Guid RecipientUserId { get; set; }
    public string RecipientEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? NotificationType { get; set; } // "welcome", "pet_created", etc.
}

/// <summary>
/// Raised when a welcome email should be sent to a newly registered user.
/// Published by Identity Service.
/// Subscribed by Notification Service.
/// </summary>
public class SendWelcomeEmailEvent : DomainEventNotification
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
