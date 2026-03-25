using Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetService.Application.Commands;
using PetService.Application.DTOs;
using PetService.Application.Queries;
using SharedKernel;

namespace PetService.Api.Controllers;

/// <summary>
/// API endpoints for managing pets.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class PetsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PetsController> _logger;

    public PetsController(IMediator mediator, ILogger<PetsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new pet.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PetDto>> CreatePet(
        [FromBody] CreatePetRequest request,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Creating pet for owner {OwnerId}. Pet: Name={Name}, Type={Type}, DateOfBirth={DateOfBirth}",
                ownerId, request.Name, request.Type, request.DateOfBirth);

            var command = new CreatePetCommand(
                ownerId,
                request.Name,
                request.Type,
                request.Breed,
                request.DateOfBirth,
                request.Description);

            var result = await _mediator.Send(command, cancellationToken);
            _logger.LogInformation("Pet created successfully with ID {PetId}", result.Id);
            return CreatedAtAction(nameof(GetPet), new { petId = result.Id, ownerId }, result);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation exception when creating pet for owner {OwnerId}: {Message}", ownerId, ex.Message);
            return BadRequest(new { error = ex.Message, code = ex.Code, errors = ex.Errors });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception when creating pet for owner {OwnerId}: {Message}", ownerId, ex.Message);
            return BadRequest(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error creating pet for owner {OwnerId}: {Message}", ownerId, ex.Message);
            return StatusCode(500, new { error = "An error occurred while creating the pet", code = "INTERNAL_ERROR" });
        }
    }

    /// <summary>
    /// Gets a pet by ID.
    /// </summary>
    [HttpGet("{petId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PetDto>> GetPet(
        [FromRoute] Guid petId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetPetQuery(petId, ownerId);
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
            _logger.LogError(ex, "Error getting pet {PetId}", petId);
            return StatusCode(500, new { error = "An error occurred while retrieving the pet" });
        }
    }

    /// <summary>
    /// Gets all pets for an owner with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<PetDto>>> GetPets(
        [FromQuery] Guid ownerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new GetOwnerPetsQuery(ownerId, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pets for owner {OwnerId}", ownerId);
            return StatusCode(500, new { error = "An error occurred while retrieving pets" });
        }
    }

    /// <summary>
    /// Searches pets by name.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<PetDto>>> SearchPets(
        [FromQuery] Guid ownerId,
        [FromQuery] string searchTerm,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = new SearchPetsQuery(ownerId, searchTerm, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching pets for owner {OwnerId}", ownerId);
            return StatusCode(500, new { error = "An error occurred while searching pets" });
        }
    }

    /// <summary>
    /// Updates a pet.
    /// </summary>
    [HttpPut("{petId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PetDto>> UpdatePet(
        [FromRoute] Guid petId,
        [FromQuery] Guid ownerId,
        [FromBody] UpdatePetRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdatePetCommand(petId, ownerId, request.Name, request.Breed, request.Description);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Pet not found: {PetId}", petId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception when updating pet {PetId}", petId);
            return BadRequest(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating pet {PetId}", petId);
            return StatusCode(500, new { error = "An error occurred while updating the pet" });
        }
    }

    /// <summary>
    /// Deletes a pet.
    /// </summary>
    [HttpDelete("{petId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePet(
        [FromRoute] Guid petId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new DeletePetCommand(petId, ownerId);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Pet not found: {PetId}", petId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting pet {PetId}", petId);
            return StatusCode(500, new { error = "An error occurred while deleting the pet" });
        }
    }
}
