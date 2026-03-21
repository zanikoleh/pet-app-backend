namespace SharedKernel.Infrastructure;

/// <summary>
/// Generic repository interface defining the contract for data access operations.
/// </summary>
/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
/// <typeparam name="TId">The type of the entity's identifier.</typeparam>
public interface IRepository<TEntity, TId> where TEntity : class
{
    /// <summary>
    /// Gets an entity by its identifier.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity to the repository.
    /// </summary>
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an entity by its identifier.
    /// </summary>
    Task DeleteAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves changes to the underlying data store.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an entity exists by its identifier.
    /// </summary>
    Task<bool> ExistsAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds an entity based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the entity.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The entity that matches the specification, or null if not found.</returns>
    Task<TEntity?> FindAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a list of entities based on a specification.
    /// </summary>
    /// <param name="specification">The specification to filter the entities.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The count of entities that match the specification.</returns>
    Task<int> CountAsync(Specification<TEntity> specification, CancellationToken cancellationToken = default);
}
