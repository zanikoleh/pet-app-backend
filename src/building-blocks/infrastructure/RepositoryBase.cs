using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Infrastructure;

/// <summary>
/// Generic base repository class providing standard CRUD operations with Entity Framework Core.
/// Supports entities that inherit from Entity{TId}.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
/// <typeparam name="TDbContext">The DbContext type used for data access.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class RepositoryBase<TEntity, TDbContext, TId> 
    where TEntity : class
    where TDbContext : DbContext
    where TId : notnull
{
    protected readonly TDbContext _dbContext;

    protected RepositoryBase(TDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }



    public Task<int> CountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return DbSet.Where(x => specification.IsSatisfiedBy(x)).CountAsync(cancellationToken);
    }

    public Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default)
    {
        return DbSet.Where(x => specification.IsSatisfiedBy(x)).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Gets a DbSet for the entity type.
    /// </summary>
    protected virtual DbSet<TEntity> DbSet => _dbContext.Set<TEntity>();

    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    public virtual async Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Gets all entities.
    /// </summary>
    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
        return entity;
    }

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    public virtual async Task DeleteAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
        }
    }

    /// <summary>
    /// Saves changes to the database.
    /// Derived classes can override this to implement additional logic such as domain event publishing.
    /// </summary>
    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if an entity exists by its identifier.
    /// </summary>
    public virtual async Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        return entity != null;
    }
}

/// <summary>
/// Generic base repository class for entities that don't require separate DbContext type parameter.
/// Use this when the DbContext is defined in the same service/project.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public abstract class RepositoryBase<TEntity, TId> : RepositoryBase<TEntity, DbContext, TId>
    where TEntity : class
    where TId : notnull
{
    protected RepositoryBase(DbContext dbContext) : base(dbContext)
    {
    }

    protected RepositoryBase(DbContext dbContext, ILogger logger) : base(dbContext)
    {
    }
}
