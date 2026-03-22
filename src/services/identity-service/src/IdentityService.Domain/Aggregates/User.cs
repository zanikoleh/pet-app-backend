using IdentityService.Domain.Entities;
using IdentityService.Domain.Events;
using IdentityService.Domain.ValueObjects;
using SharedKernel;

namespace IdentityService.Domain.Aggregates;

public sealed class User : AggregateRoot<Guid>
{
    public Email Email { get; private set; } = null!;
    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Avatar { get; private set; }
    public PasswordHash? PasswordHash { get; private set; }
    public Role Role { get; private set; } = Role.User;
    public bool IsActive { get; private set; } = true;
    public bool IsEmailConfirmed { get; private set; }
    public DateTime LastLoginAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private readonly List<OAuthProvider> _oauthProviders = new();
    private readonly List<RefreshToken> _refreshTokens = new();

    public IReadOnlyCollection<OAuthProvider> OAuthProviders => _oauthProviders;
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens;

    private User() { }

    public User(Email email, PasswordHash? passwordHash = null, string? fullName = null)
        : base(Guid.NewGuid())
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash;
        IsEmailConfirmed = passwordHash == null; // OAuth users auto-confirmed
        CreatedAt = DateTime.UtcNow;

        var (firstName, lastName) = ParseFullName(fullName);
        FirstName = firstName;
        LastName = lastName;

        RaiseDomainEvent(new UserRegisteredEvent
        {
            UserId = Id,
            Email = Email.Value,
            FirstName = FirstName,
            LastName = LastName,
            Avatar = Avatar
        });
    }

    public static User CreateFromOAuth(
        string provider,
        string providerUserId,
        Email email,
        string? fullName = null,
        string? avatar = null)
    {
        var user = new User(email, null, fullName);
        user.Avatar = avatar;
        user.LinkOAuthProvider(provider, providerUserId);
        return user;
    }

    public void LinkOAuthProvider(string provider, string providerUserId)
    {
        if (_oauthProviders.Any(p => p.Provider == provider))
            throw new DomainException($"OAuth provider {provider} is already linked.");

        _oauthProviders.Add(new OAuthProvider(provider, providerUserId));
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new OAuthProviderLinkedEvent
        {
            UserId = Id,
            Email = Email.Value,
            Provider = provider,
            ProviderUserId = providerUserId
        });
    }

    public void UnlinkOAuthProvider(string provider)
    {
        var oauthProvider = _oauthProviders.FirstOrDefault(p => p.Provider == provider);
        if (oauthProvider != null)
        {
            _oauthProviders.Remove(oauthProvider);
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public bool HasOAuthProvider(string provider, string providerUserId)
    {
        return _oauthProviders.Any(p => p.Provider == provider && p.ProviderUserId == providerUserId);
    }

    public bool VerifyPassword(string password)
    {
        if (PasswordHash == null)
            return false;
        return PasswordHash.Verify(password);
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserPasswordChangedEvent
        {
            UserId = Id,
            Email = Email.Value
        });
    }

    public void UpdateProfile(string? firstName, string? lastName, string? avatar)
    {
        (FirstName, LastName) = ParseFullName($"{firstName} {lastName}".Trim());
        Avatar = avatar;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserProfileUpdatedEvent
        {
            UserId = Id,
            Email = Email.Value
        });
    }

    public void VerifyEmail()
    {
        IsEmailConfirmed = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRefreshToken(string token, int expirationMinutes)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);
        _refreshTokens.Add(new RefreshToken(Id, token, expiresAt));
        UpdatedAt = DateTime.UtcNow;
    }

    public RefreshToken? GetValidRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsValid);
    }

    public void RevokeRefreshToken(Guid token) => RevokeRefreshToken(token.ToString());

    public void RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        if (refreshToken != null)
        {
            refreshToken.IsRevoked = true;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new UserDeactivatedEvent
        {
            UserId = Id,
            Email = Email.Value
        });
    }

    private static (string? firstName, string? lastName) ParseFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (null, null);

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var firstName = parts.Length > 0 ? parts[0] : null;
        var lastName = parts.Length > 1 ? string.Join(" ", parts.Skip(1)) : null;

        return (firstName, lastName);
    }
}