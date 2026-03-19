namespace SharedKernel;

/// <summary>
/// Base class for specifications used to build complex queries.
/// Implements the Specification design pattern for DDD.
/// </summary>
public abstract class Specification<T>
{
    public IQueryable<T>? Query { get; protected set; }

    protected virtual void BuildQuery(IQueryable<T> query)
    {
    }

    public virtual Func<T, bool> ToFunc()
    {
        return _ => true;
    }
}

/// <summary>
/// Base specification for Entity Framework queries with Include, OrderBy, Paging.
/// </summary>
public abstract class Specification<T, TResult> where T : class
{
    public Expression<Func<T, TResult>>? Selector { get; protected set; }

    public Expression<Func<T, bool>>? Criteria { get; protected set; }

    public List<Expression<Func<T, object>>> Includes { get; } = new();

    public List<string> IncludeStrings { get; } = new();

    public Expression<Func<T, object>>? OrderBy { get; protected set; }

    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }

    public int? Take { get; protected set; }

    public int? Skip { get; protected set; }

    public bool IsPagingEnabled { get; protected set; }

    protected virtual void AddInclude(Expression<Func<T, object>> includeExpression)
    {
        Includes.Add(includeExpression);
    }

    protected virtual void AddInclude(string includeString)
    {
        IncludeStrings.Add(includeString);
    }

    protected virtual void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
        IsPagingEnabled = true;
    }

    protected virtual void ApplyOrdering(Expression<Func<T, object>> orderByExpression)
    {
        OrderBy = orderByExpression;
    }

    protected virtual void ApplyOrderingDescending(Expression<Func<T, object>> orderByDescendingExpression)
    {
        OrderByDescending = orderByDescendingExpression;
    }
}
