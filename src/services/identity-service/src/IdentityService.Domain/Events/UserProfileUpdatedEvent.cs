using SharedKernel;

namespace IdentityService.Domain.Events;

public sealed class UserProfileUpdatedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}