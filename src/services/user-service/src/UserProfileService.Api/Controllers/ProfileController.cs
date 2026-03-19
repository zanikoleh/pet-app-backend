using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Application.Queries;
using UserProfileService.Application.DTOs;

namespace UserProfileService.Api.Controllers;

/// <summary>
/// Controller for user profile management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProfileController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Get user profile by ID.
    /// </summary>
    [HttpGet("{userProfileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfile(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserProfileQuery(userProfileId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get user profile by UserId.
    /// </summary>
    [HttpGet("by-user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetProfileByUserId(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetUserProfileByUserIdQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update user profile.
    /// </summary>
    [HttpPut("{userProfileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> UpdateProfile(
        Guid userProfileId,
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateProfileCommand(
            userProfileId,
            request.FirstName,
            request.LastName,
            request.Bio,
            request.DateOfBirth,
            request.PhoneNumber,
            request.Address,
            request.City,
            request.Country,
            request.ProfilePictureUrl);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get notification preferences.
    /// </summary>
    [HttpGet("{userProfileId}/notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationPreferencesDto>> GetNotificationPreferences(
        Guid userProfileId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetNotificationPreferencesQuery(userProfileId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update notification preferences.
    /// </summary>
    [HttpPut("{userProfileId}/notifications")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationPreferencesDto>> UpdateNotificationPreferences(
        Guid userProfileId,
        [FromBody] UpdateNotificationPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateNotificationPreferencesCommand(
            userProfileId,
            request.EmailNotifications,
            request.PushNotifications,
            request.SmsNotifications,
            request.ReceivePromotions,
            request.ReceiveNewsletter);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update language and timezone.
    /// </summary>
    [HttpPut("{userProfileId}/language")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateLanguage(
        Guid userProfileId,
        [FromBody] UpdateLanguageRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateLanguageAndTimezoneCommand(userProfileId, request.Language, request.Timezone);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
