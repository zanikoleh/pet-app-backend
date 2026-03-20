using SharedKernel;

namespace IdentityService.Domain.ValueObjects;

public sealed class Email : ValueObject
{
    public string Value { get; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("@"))
            throw new DomainException("Invalid email format.");
        Value = value;
    }

    public static Email Create(string email)
    {
        return new Email(email);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value.ToLower();
    }
}