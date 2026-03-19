using MediatR;
using Microsoft.AspNetCore.Mvc;
using PetService.Application.Commands;
using PetService.Application.DTOs;
using PetService.Application.Queries;

namespace PetService.Api.Controllers;

/// <summary>
/// API endpoints for managing pet documents.
/// </summary>
[ApiController]
[Route("api/pets/{petId}/documents")]
[Produces("application/json")]
public sealed class PetDocumentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PetDocumentsController> _logger;

    public PetDocumentsController(IMediator mediator, ILogger<PetDocumentsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets all documents of a pet.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<DocumentDto>>> GetDocuments(
        [FromRoute] Guid petId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetPetDocumentsQuery(petId, ownerId);
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
            _logger.LogError(ex, "Error getting documents for pet {PetId}", petId);
            return StatusCode(500, new { error = "An error occurred while retrieving documents" });
        }
    }

    /// <summary>
    /// Gets a specific document.
    /// </summary>
    [HttpGet("{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetDocument(
        [FromRoute] Guid petId,
        [FromRoute] Guid documentId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetDocumentQuery(petId, documentId, ownerId);
            var result = await _mediator.Send(query, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Document not found: {DocumentId}", documentId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {DocumentId}", documentId);
            return StatusCode(500, new { error = "An error occurred while retrieving the document" });
        }
    }

    /// <summary>
    /// Adds a document to a pet.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> AddDocument(
        [FromRoute] Guid petId,
        [FromQuery] Guid ownerId,
        [FromBody] AddDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new AddDocumentToPetCommand(
                petId,
                ownerId,
                request.FileName,
                request.FileType,
                request.FileSizeBytes,
                request.Url,
                request.Category,
                request.Description);

            var result = await _mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetDocument), new { petId, documentId = result.Id, ownerId }, result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Pet not found: {PetId}", petId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain exception when adding document to pet {PetId}", petId);
            return BadRequest(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding document to pet {PetId}", petId);
            return StatusCode(500, new { error = "An error occurred while adding the document" });
        }
    }

    /// <summary>
    /// Updates a document.
    /// </summary>
    [HttpPut("{documentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> UpdateDocument(
        [FromRoute] Guid petId,
        [FromRoute] Guid documentId,
        [FromQuery] Guid ownerId,
        [FromBody] UpdateDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new UpdatePetDocumentCommand(petId, documentId, ownerId, request.Description);
            var result = await _mediator.Send(command, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Document not found: {DocumentId}", documentId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {DocumentId}", documentId);
            return StatusCode(500, new { error = "An error occurred while updating the document" });
        }
    }

    /// <summary>
    /// Deletes a document.
    /// </summary>
    [HttpDelete("{documentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDocument(
        [FromRoute] Guid petId,
        [FromRoute] Guid documentId,
        [FromQuery] Guid ownerId,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RemoveDocumentFromPetCommand(petId, documentId, ownerId);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Document not found: {DocumentId}", documentId);
            return NotFound(new { error = ex.Message, code = ex.Code });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return StatusCode(500, new { error = "An error occurred while deleting the document" });
        }
    }
}
