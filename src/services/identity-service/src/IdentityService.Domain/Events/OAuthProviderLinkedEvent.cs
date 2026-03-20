using SharedKernel;

namespace IdentityService.Domain.Events;

public sealed class OAuthProviderLinkedEvent : DomainEvent
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
}