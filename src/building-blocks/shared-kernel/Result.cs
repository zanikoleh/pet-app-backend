namespace SharedKernel;

/// <summary>
/// Represents the result of an operation.
/// Can contain a success value or a failure reason.
/// Used to avoid exceptions for expected failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? Code { get; }

    protected Result(bool success, string? error = null, string? code = null)
    {
        IsSuccess = success;
        Error = error;
        Code = code;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error, string? code = null) => new(false, error, code);

    public static Result<T> Success<T>(T value) => new(true, value, null, null);
    public static Result<T> Failure<T>(string error, string? code = null) => new(false, default, error, code);
}

/// <summary>
/// Represents the result of an operation that can return a value or a failure.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    protected internal Result(bool success, T? value, string? error = null, string? code = null)
        : base(success, error, code)
    {
        Value = value;
    }

    public new static Result<T> Success(T value) => new(true, value);
    public new static Result<T> Failure(string error, string? code = null) => new(false, default, error, code);

    /// <summary>
    /// Maps the success value to another type.
    /// </summary>
    public Result<TNew> Map<TNew>(Func<T?, TNew> mapping)
    {
        return IsSuccess
            ? Result<TNew>.Success(mapping(Value))
            : Result<TNew>.Failure(Error ?? "Unknown error", Code);
    }

    /// <summary>
    /// Chains another result-returning operation.
    /// </summary>
    public Result<TNew> Bind<TNew>(Func<T?, Result<TNew>> binding)
    {
        return IsSuccess
            ? binding(Value)
            : Result<TNew>.Failure(Error ?? "Unknown error", Code);
    }

    /// <summary>
    /// Executes an action on success.
    /// </summary>
    public Result<T> Tap(Action<T?> action)
    {
        if (IsSuccess)
            action(Value);

        return this;
    }

    /// <summary>
    /// Returns the value or throws an exception if failed.
    /// </summary>
    public T? GetValueOrThrow()
    {
        if (IsFailure)
            throw new DomainException(Error ?? "Operation failed", Code);

        return Value;
    }
}
