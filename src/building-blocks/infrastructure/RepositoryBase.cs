using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure;

/// <summary>
/// Generic repository base class implementing the repository pattern.
/// Provides common CRUD operations and querying methods.
/// </summary>
public abstract class RepositoryBase<T, TId> : IRepository<T, TId>
    where T : AggregateRoot<TId>
    where TId : notnull
{
    protected readonly ApplicationDbContextBase DbContext;
    protected readonly DbSet<T> DbSet;
    protected readonly ILogger<RepositoryBase<T, TId>> Logger;

    protected RepositoryBase(
        ApplicationDbContextBase dbContext,
        ILogger<RepositoryBase<T, TId>> logger)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        DbSet = dbContext.Set<T>();
    }

    /// <summary>
    /// Adds a new aggregate to the repository.
    /// </summary>
    public virtual void Add(T aggregate)
    {
        DbSet.Add(aggregate);
        Logger.LogDebug("Added aggregate of type {AggregateType} with id {Id}", typeof(T).Name, aggregate.Id);
    }

    /// <summary>
    /// Adds multiple aggregates to the repository.
    /// </summary>
    public virtual void AddRange(IEnumerable<T> aggregates)
    {
        DbSet.AddRange(aggregates);
        Logger.LogDebug("Added {Count} aggregates of type {AggregateType}", aggregates.Count(), typeof(T).Name);
    }

    /// <summary>
    /// Updates an existing aggregate in the repository.
    /// </summary>
    public virtual void Update(T aggregate)
    {
        DbSet.Update(aggregate);
        Logger.LogDebug("Updated aggregate of type {AggregateType} with id {Id}", typeof(T).Name, aggregate.Id);
    }

    /// <summary>
    /// Removes an aggregate from the repository.
    /// </summary>
    public virtual void Remove(T aggregate)
    {
        DbSet.Remove(aggregate);
        Logger.LogDebug("Removed aggregate of type {AggregateType} with id {Id}", typeof(T).Name, aggregate.Id);
    }

    /// <summary>
    /// Gets an aggregate by its ID.
    /// Returns null if not found.
    /// </summary>
    public virtual async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        var aggregate = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        return aggregate;
    }

    /// <summary>
    /// Gets all aggregates of this type.
    /// </summary>
    public virtual async Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets aggregates matching a specification.
    /// </summary>
    public virtual async Task<IReadOnlyCollection<T>> FindAsync(
        Specification<T, T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Counts aggregates matching a specification.
    /// </summary>
    public virtual async Task<int> CountAsync(
        Specification<T, T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.CountAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if any aggregate matches a specification.
    /// </summary>
    public virtual async Task<bool> AnyAsync(
        Specification<T, T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.AnyAsync(cancellationToken);
    }

    /// <summary>
    /// Applies a specification to get a single aggregate, or null if not found.
    /// </summary>
    public virtual async Task<T?> FirstOrDefaultAsync(
        Specification<T, T> specification,
        CancellationToken cancellationToken = default)
    {
        var query = ApplySpecification(specification);
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Saves changes to the database.
    /// </summary>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Applies specification to a queryable.
    /// Override in derived classes for advanced query customization.
    /// </summary>
    protected virtual IQueryable<T> ApplySpecification(Specification<T, T> specification)
    {
        var query = DbSet.AsQueryable();

        // Apply criteria
        if (specification.Criteria != null)
            query = query.Where(specification.Criteria);

        // Apply includes
        if (specification.Includes.Any())
            query = specification.Includes.Aggregate(query, (current, include) => current.Include(include));

        // Apply string-based includes
        if (specification.IncludeStrings.Any())
            query = specification.IncludeStrings.Aggregate(query, (current, includeString) => current.Include(includeString));

        // Apply ordering
        if (specification.OrderBy != null)
            query = query.OrderBy(specification.OrderBy);
        else if (specification.OrderByDescending != null)
            query = query.OrderByDescending(specification.OrderByDescending);

        // Apply paging
        if (specification.IsPagingEnabled && specification.Skip.HasValue && specification.Take.HasValue)
            query = query.Skip(specification.Skip.Value).Take(specification.Take.Value);

        return query;
    }
}

/// <summary>
/// Interface for the repository pattern. All repositories should implement this.
/// </summary>
public interface IRepository<T, TId> where T : AggregateRoot<TId> where TId : notnull
{
    void Add(T aggregate);
    void AddRange(IEnumerable<T> aggregates);
    void Update(T aggregate);
    void Remove(T aggregate);
    Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<T>> FindAsync(Specification<T, T> specification, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Specification<T, T> specification, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Specification<T, T> specification, CancellationToken cancellationToken = default);
    Task<T?> FirstOrDefaultAsync(Specification<T, T> specification, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
