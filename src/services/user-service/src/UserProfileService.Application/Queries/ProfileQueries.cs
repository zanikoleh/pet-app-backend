using MediatR;
using UserProfileService.Application.DTOs;

namespace UserProfileService.Application.Queries;

/// <summary>
/// Query to get user profile by ID.
/// </summary>
public sealed record GetUserProfileQuery(Guid UserProfileId) : IRequest<UserProfileDto>;

/// <summary>
/// Query to get user profile by UserId.
/// </summary>
public sealed record GetUserProfileByUserIdQuery(Guid UserId) : IRequest<UserProfileDto>;

/// <summary>
/// Query to get notification preferences.
/// </summary>
public sealed record GetNotificationPreferencesQuery(Guid UserProfileId) : IRequest<NotificationPreferencesDto>;
