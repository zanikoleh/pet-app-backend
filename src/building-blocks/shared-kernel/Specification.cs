using System.Linq.Expressions;

namespace SharedKernel;

public abstract class Specification<T>
{
    /// <summary>
    /// The criteria expression that defines this specification (translatable to SQL by EF Core)
    /// </summary>
    protected Expression<Func<T, bool>>? Criteria { get; set; }
    
    /// <summary>
    /// Optional ordering function for results
    /// </summary>
    protected Func<T, object>? OrderByDescending { get; set; }

    /// <summary>
    /// Paging information
    /// </summary>
    protected int? Skip { get; private set; }
    protected int? Take { get; private set; }

    /// <summary>
    /// Gets the criteria expression for this specification
    /// </summary>
    public virtual Expression<Func<T, bool>>? GetExpression() => Criteria;

    /// <summary>
    /// Checks if the given object satisfies this specification
    /// </summary>
    public virtual bool IsSatisfiedBy(T candidate)
    {
        if (Criteria == null)
            return true;
        
        var compiled = Criteria.Compile();
        return compiled.Invoke(candidate);
    }
    
    /// <summary>
    /// Applies paging to the specification
    /// </summary>
    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    /// <summary>
    /// Gets the number of records to skip for pagination
    /// </summary>
    public int? GetSkip() => Skip;

    /// <summary>
    /// Gets the number of records to take for pagination
    /// </summary>
    public int? GetTake() => Take;
}

