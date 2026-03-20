using SharedKernel;

namespace IdentityService.Domain.ValueObjects;

public sealed class Role : ValueObject
{
    public string Value { get; }

    public static readonly Role User = new("User");
    public static readonly Role Admin = new("Admin");

    private Role(string value)
    {
        Value = value;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}