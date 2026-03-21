using SharedKernel;

namespace PetService.Domain.ValueObjects;

/// <summary>
/// Value object for pet type (dog, cat, bird, rabbit, etc.)
/// </summary>
public sealed class PetType : ValueObject
{
    public static readonly PetType Dog = new("dog");
    public static readonly PetType Cat = new("cat");
    public static readonly PetType Bird = new("bird");
    public static readonly PetType Rabbit = new("rabbit");
    public static readonly PetType Hamster = new("hamster");
    public static readonly PetType Fish = new("fish");
    public static readonly PetType Snake = new("snake");
    public static readonly PetType Other = new("other");

    public string Value { get; }

    private PetType(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Pet type cannot be empty.");

        Value = value.ToLowerInvariant();
    }

    public static PetType Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Pet type cannot be empty.");

        return new PetType(value);
    }

    public static PetType FromString(string value)
    {
        var knownTypes = new[] { Dog, Cat, Bird, Rabbit, Hamster, Fish, Snake, Other };
        var type = knownTypes.FirstOrDefault(t => t.Value == value.ToLowerInvariant());
        return type ?? new PetType(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

/// <summary>
/// Value object for pet breed
/// </summary>
public sealed class Breed : ValueObject
{
    public string Value { get; }

    private Breed(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Breed cannot be empty.");

        if (value.Length > 100)
            throw new DomainException("Breed cannot exceed 100 characters.");

        Value = value;
    }

    public static Breed Create(string value)
    {
        return new Breed(value);
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
