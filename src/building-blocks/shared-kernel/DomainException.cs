namespace SharedKernel;

/// <summary>
/// Base exception for domain-related errors.
/// Thrown when a domain invariant is violated.
/// </summary>
public class DomainException : Exception
{
    public string? Code { get; }

    public DomainException(string message, string? code = null)
        : base(message)
    {
        Code = code;
    }

    public DomainException(string message, Exception innerException, string? code = null)
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when trying to execute a command but a validation rule is broken.
/// </summary>
public class ValidationException : DomainException
{
    public IReadOnlyCollection<string> Errors { get; }

    public ValidationException(
        string message,
        IReadOnlyCollection<string> errors,
        string? code = null)
        : base(message, code)
    {
        Errors = errors;
    }

    public ValidationException(string error, string? code = null)
        : base(error, code)
    {
        Errors = new[] { error };
    }
}

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : DomainException
{
    public NotFoundException(string message, string? code = null)
        : base(message, code)
    {
    }

    public static NotFoundException For<T>(object id) where T : Entity<int>
        => new(typeof(T).Name + " with id " + id + " not found.", "NOT_FOUND");

    public static NotFoundException For<T, TId>(TId id) where T : Entity<TId> where TId : notnull
        => new(typeof(T).Name + " with id " + id + " not found.", "NOT_FOUND");
}

/// <summary>
/// Exception thrown when trying to execute a command that violates business logic.
/// </summary>
public class BusinessLogicException : DomainException
{
    public BusinessLogicException(string message, string? code = null)
        : base(message, code)
    {
    }
}
