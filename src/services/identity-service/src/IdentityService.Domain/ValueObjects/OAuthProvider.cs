using SharedKernel;

namespace IdentityService.Domain.ValueObjects;

public sealed class OAuthProvider : ValueObject
{
    public string Provider { get; }
    public string ProviderUserId { get; }
    public DateTime LinkedAt { get; }

    public OAuthProvider(string provider, string providerUserId)
    {
        Provider = provider;
        ProviderUserId = providerUserId;
        LinkedAt = DateTime.UtcNow;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Provider;
        yield return ProviderUserId;
    }
}