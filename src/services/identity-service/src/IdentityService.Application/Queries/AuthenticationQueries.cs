using MediatR;
using IdentityService.Application.DTOs;

namespace IdentityService.Application.Queries;

/// <summary>
/// Query to get user profile by ID.
/// </summary>
public sealed record GetUserProfileQuery(Guid UserId) : IRequest<UserDto>;

/// <summary>
/// Query to check if email exists.
/// </summary>
public sealed record EmailExistsQuery(string Email) : IRequest<bool>;

/// <summary>
/// Query to validate access token (for verification).
/// </summary>
public sealed record ValidateAccessTokenQuery(string AccessToken) : IRequest<Guid?>;
