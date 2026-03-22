using Microsoft.EntityFrameworkCore;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Aggregates;
using IdentityService.Domain.ValueObjects;
using SharedKernel.Infrastructure;

namespace IdentityService.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for User aggregate.
/// </summary>
public class UserRepository : RepositoryBase<User, IdentityServiceDbContext, Guid>, IUserRepository
{
    public UserRepository(IdentityServiceDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = _dbContext.Users
            .AsEnumerable()
            .FirstOrDefault(u => u.Email.Value.ToLowerInvariant() == normalizedEmail);
        return await Task.FromResult(user);
    }

    public async Task<User?> GetByOAuthProviderAsync(
        string provider,
        string providerUserId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.OAuthProviders
                .Any(ol => ol.Provider == provider && ol.ProviderUserId == providerUserId),
                cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var exists = _dbContext.Users
            .AsEnumerable()
            .Any(u => u.Email.Value.ToLowerInvariant() == normalizedEmail);
        return await Task.FromResult(exists);
    }
}
