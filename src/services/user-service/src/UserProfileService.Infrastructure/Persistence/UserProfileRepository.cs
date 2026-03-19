using Microsoft.EntityFrameworkCore;
using UserProfileService.Application.Interfaces;
using UserProfileService.Domain.Aggregates;
using SharedKernel.Infrastructure;

namespace UserProfileService.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for UserProfile aggregate.
/// </summary>
public class UserProfileRepository : RepositoryBase<UserProfile, UserProfileServiceDbContext, Guid>, IUserProfileRepository
{
    public UserProfileRepository(UserProfileServiceDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.UserProfiles
            .FirstOrDefaultAsync(up => up.UserId == userId, cancellationToken);
    }
}
