using SharedKernel;

namespace IdentityService.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRevoked { get; set; }

    private RefreshToken() { }

    public RefreshToken(Guid userId, string token, DateTime expiresAt)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        CreatedAt = DateTime.UtcNow;
        IsRevoked = false;
    }

    public bool IsValid => !IsRevoked && DateTime.UtcNow < ExpiresAt;
}