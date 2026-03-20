using System.Security.Cryptography;
using System.Text;
using SharedKernel;

namespace IdentityService.Domain.ValueObjects;

public sealed class PasswordHash : ValueObject
{
    public string Value { get; }

    private PasswordHash(string hash)
    {
        Value = hash;
    }

    public static PasswordHash Create(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new DomainException("Password must be at least 8 characters.");
        
        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        return new PasswordHash(hash);
    }

    public bool Verify(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, Value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}