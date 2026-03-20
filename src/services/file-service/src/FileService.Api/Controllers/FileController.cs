using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using FileService.Application.Commands;
using FileService.Application.Queries;
using FileService.Application.DTOs;

namespace FileService.Api.Controllers;

/// <summary>
/// Controller for file operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Authorize]
public class FileController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Constructor for FileController.
    /// </summary>
    public FileController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Upload a file.
    /// </summary>
    [HttpPost("upload")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<FileDto>> UploadFile(
        IFormFile file,
        [FromForm] string? category,
        [FromForm] Guid? relatedEntityId,
        CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file provided.");

        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream, cancellationToken);
        var fileBytes = memoryStream.ToArray();

        var command = new UploadFileCommand(
            parsedUserId,
            file.FileName,
            file.ContentType ?? "application/octet-stream",
            fileBytes,
            category,
            relatedEntityId);

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get file by ID.
    /// </summary>
    [HttpGet("{fileId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FileDto>> GetFile(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFileQuery(fileId);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get file download URL.
    /// </summary>
    [HttpGet("{fileId}/download-url")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DownloadUrlResponse>> GetDownloadUrl(
        Guid fileId,
        [FromQuery] int expirationMinutes = 60,
        CancellationToken cancellationToken = default)
    {
        var query = new GetFileDownloadUrlQuery(fileId, expirationMinutes);
        var url = await _mediator.Send(query, cancellationToken);
        return Ok(new DownloadUrlResponse { DownloadUrl = url });
    }

    /// <summary>
    /// Delete file.
    /// </summary>
    [HttpDelete("{fileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteFile(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var command = new DeleteFileCommand(fileId, parsedUserId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// List user files.
    /// </summary>
    [HttpGet("user/files")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedListDto<FileDto>>> ListUserFiles(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userId == null || !Guid.TryParse(userId.Value, out var parsedUserId))
            return Unauthorized("Invalid or missing user ID");

        var query = new ListUserFilesQuery(parsedUserId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// List files by related entity.
    /// </summary>
    [HttpGet("entity/{relatedEntityId}/files")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginatedListDto<FileDto>>> ListFilesByEntity(
        Guid relatedEntityId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new ListFilesByEntityQuery(relatedEntityId, pageNumber, pageSize);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

/// <summary>
/// Response containing download URL.
/// </summary>
public class DownloadUrlResponse
{
    /// <summary>
    /// The URL to download the file. This URL is typically time-limited and may require authentication depending on the storage implementation.
    /// </summary>
    public string DownloadUrl { get; set; } = string.Empty;
}
