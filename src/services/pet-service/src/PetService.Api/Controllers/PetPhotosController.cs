using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetService.Application.Commands;
using PetService.Application.DTOs;
using PetService.Application.Queries;

namespace PetService.Api.Controllers;

/// <summary>
/// API endpoints for managing pet photos.
/// </summary>
[ApiController]
[Route("api/pets/{petId}/photos")]
[Produces("application/json")]
public sealed class PetPhotosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PetPhotosController> _logger;

    public PetPhotosController(IMediator mediator, ILogger<PetPhotosController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all photos of a pet.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<PhotoDto>>> GetPhotos(
        [FromRoute] Guid petId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetPetPhotosQuery(petId, ownerId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Pet not found: {PetId}", petId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting photos for pet {PetId}", petId);
            return StatusCode(500, new { error = "An error occurred while retrieving photos" });
        }
    }

    /// <summary>
    /// Gets a specific photo.
    /// </summary>
    [HttpGet("{photoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoDto>> GetPhoto(
        [FromRoute] Guid petId,
        [FromRoute] Guid photoId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetPhotoQuery(petId, photoId, ownerId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo not found: {PhotoId}", photoId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting photo {PhotoId}", photoId);
            return StatusCode(500, new { error = "An error occurred while retrieving the photo" });
        }
    }

    /// <summary>
    /// Adds a photo to a pet.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PhotoDto>> AddPhoto(
        [FromRoute] Guid petId,
        [FromQuery] Guid ownerId,
        [FromBody] AddPhotoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddPhotoToPetCommand(
                petId,
                ownerId,
                request.FileName,
                request.FileType,
                request.FileSizeBytes,
                request.Url,
                request.Caption,
                request.Tags);

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetPhoto), new { petId, photoId = result.Id, ownerId }, result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Pet not found: {PetId}", petId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception when adding photo to pet {PetId}", petId);
            return BadRequest(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding photo to pet {PetId}", petId);
            return StatusCode(500, new { error = "An error occurred while adding the photo" });
        }
    }

    /// <summary>
    /// Updates a photo.
    /// </summary>
    [HttpPut("{photoId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PhotoDto>> UpdatePhoto(
        [FromRoute] Guid petId,
        [FromRoute] Guid photoId,
        [FromQuery] Guid ownerId,
        [FromBody] UpdatePhotoRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdatePetPhotoCommand(petId, photoId, ownerId, request.Caption, request.Tags);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo not found: {PhotoId}", photoId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating photo {PhotoId}", photoId);
            return StatusCode(500, new { error = "An error occurred while updating the photo" });
        }
    }

    /// <summary>
    /// Deletes a photo.
    /// </summary>
    [HttpDelete("{photoId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePhoto(
        [FromRoute] Guid petId,
        [FromRoute] Guid photoId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RemovePhotoFromPetCommand(petId, photoId, ownerId);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Photo not found: {PhotoId}", photoId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting photo {PhotoId}", photoId);
            return StatusCode(500, new { error = "An error occurred while deleting the photo" });
        }
    }
}
