using UserProfileService.Domain.Aggregates;
using SharedKernel.Infrastructure;

namespace UserProfileService.Application.Interfaces;

/// <summary>
/// Repository interface for UserProfile aggregate.
/// </summary>
public interface IUserProfileRepository : IRepository<UserProfile, Guid>
{
    /// <summary>
    /// Gets user profile by UserId.
    /// </summary>
    Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
