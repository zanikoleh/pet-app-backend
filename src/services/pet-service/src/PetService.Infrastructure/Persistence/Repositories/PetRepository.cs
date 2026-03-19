using Microsoft.Extensions.Logging;
using PetService.Application.Handlers;
using PetService.Domain.Aggregates;

namespace PetService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Pet aggregate root.
/// </summary>
public sealed class PetRepository : RepositoryBase<Pet, Guid>, IPetRepository
{
    private readonly PetServiceDbContext _dbContext;

    public PetRepository(
        PetServiceDbContext dbContext,
        ILogger<PetRepository> logger)
        : base(dbContext, logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
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
