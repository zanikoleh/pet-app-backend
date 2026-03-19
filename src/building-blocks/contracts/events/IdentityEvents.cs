namespace Contracts.Events;

/// <summary>
/// Raised when a new user successfully registers in the system.
/// Published by Identity Service.
/// Subscribed by User Profile Service and Notification Service.
/// </summary>
public class UserRegisteredEvent : DomainEventNotification
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? SignUpMethod { get; set; } // "email", "google", "facebook", "apple"
}

/// <summary>
/// Raised when a user successfully logs in.
/// Published by Identity Service.
/// </summary>
public class UserLoggedInEvent : DomainEventNotification
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime LoginTime { get; set; }
}

/// <summary>
/// Raised when a user updates their authentication credentials or information.
/// Published by Identity Service.
/// </summary>
public class UserProfileUpdatedEvent : DomainEventNotification
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Avatar { get; set; }
}

/// <summary>
/// Raised when a user's account is deleted from the system.
/// Published by Identity Service.
/// Subscribed by all services to clean up user data.
/// </summary>
public class UserDeletedEvent : DomainEventNotification
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public DateTime DeletedAt { get; set; }
}

/// <summary>
/// Raised when a user links a social authentication provider to their account.
/// Published by Identity Service.
/// </summary>
public class SocialAuthLinkedEvent : DomainEventNotification
{
    public Guid UserId { get; set; }
    public string Provider { get; set; } = string.Empty; // "google", "facebook", "apple"
    public string ProviderUserId { get; set; } = string.Empty;
}
