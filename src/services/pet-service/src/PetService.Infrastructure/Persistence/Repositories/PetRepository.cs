using Microsoft.Extensions.Logging;
using PetService.Application.Handlers;
using PetService.Domain.Aggregates;
using SharedKernel;
using SharedKernel.Infrastructure;

namespace PetService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Pet aggregate root.
/// </summary>
public sealed class PetRepository : RepositoryBase<Pet, Guid>, IPetRepository
{
    //private readonly PetServiceDbContext _dbContext;

    public PetRepository(
        PetServiceDbContext dbContext,
        ILogger<PetRepository> logger)
        : base(dbContext, logger)
    {
        //_dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<int> CountAsync(Specification<Pet> specification, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Pet?> FindAsync(Specification<Pet> specification, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Overrides SaveChangesAsync to persist changes to the database.
    /// Domain events are automatically published after persistence.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
