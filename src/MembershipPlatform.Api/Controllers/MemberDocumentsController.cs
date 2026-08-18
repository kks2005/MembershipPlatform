using MembershipPlatform.Api.Contracts.Common;
using MembershipPlatform.Api.Contracts.Members;
using MembershipPlatform.Application;
using MembershipPlatform.Application.Members;
using Microsoft.AspNetCore.Mvc;

namespace MembershipPlatform.Api.Controllers;

[ApiController]
[Route("api/v1/members/{memberId:guid}/documents")]
public sealed class MemberDocumentsController(UploadMemberDocument uploadMemberDocument)
    : ControllerBase
{
    private const long MaximumDocumentSize = 10 * 1024 * 1024;
    private const long MaximumRequestSize = 11 * 1024 * 1024;

    /// <summary>Stores a document for an existing member.</summary>
    /// <param name="memberId">The member identifier.</param>
    /// <param name="file">The document supplied as multipart form data.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The document was stored.</response>
    /// <response code="400">The document is empty or invalid.</response>
    /// <response code="404">The member was not found.</response>
    /// <response code="413">The document exceeds the size limit.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaximumRequestSize)]
    [ProducesResponseType(typeof(MemberDocumentUploadResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Upload(
        Guid memberId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(CreateError(
                ApiErrorCodes.DocumentEmpty,
                "The document is empty."));
        }

        if (file.Length > MaximumDocumentSize)
        {
            return StatusCode(
                StatusCodes.Status413PayloadTooLarge,
                CreateError(
                    ApiErrorCodes.DocumentTooLarge,
                    "The document exceeds the 10 MB size limit."));
        }

        if (string.IsNullOrWhiteSpace(file.FileName))
        {
            return BadRequest(CreateError(
                ApiErrorCodes.DocumentInvalid,
                "The document file name is required."));
        }

        await using var content = file.OpenReadStream();
        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;
        var result = await uploadMemberDocument.ExecuteAsync(
            memberId,
            file.FileName,
            contentType,
            content,
            cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(new MemberDocumentUploadResponse(result.StorageKey!));
        }

        var error = CreateError(result.ErrorCode!, result.ErrorMessage!);
        return result.ErrorCode switch
        {
            ApplicationErrorCodes.MemberNotFound => NotFound(error),
            _ => BadRequest(error)
        };
    }

    private ApiErrorResponse CreateError(string code, string message) =>
        new(code, message, HttpContext.TraceIdentifier);
}
