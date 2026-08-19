using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Members;

/// <summary>
/// Page model for member details with multiple vertical slices:
/// - View member information
/// - Check in a member (demonstrates business rule: only active members can check in)
/// - Upload member documents (demonstrates infrastructure-independent file storage)
/// </summary>
public sealed class DetailsModel(IMembershipApiClient apiClient) : PageModel
{
    // Properties exposed to the Razor view
    public Guid MemberId { get; private set; }
    public MemberSummary? Member { get; private set; }
    public IReadOnlyList<MemberCheckIn> CheckIns { get; private set; } = [];
    public IReadOnlyList<FitnessClass> Classes { get; private set; } = [];
    public ApiError? Error { get; private set; }

    /// <summary>
    /// File upload input bound from the form.
    /// The [BindProperty] attribute enables model binding from POST requests.
    /// </summary>
    [BindProperty]
    public IFormFile? Document { get; set; }

    /// <summary>
    /// Handles GET requests to load member details.
    /// Demonstrates: Parallel API calls, single-member lookup, and null handling.
    /// </summary>
    public async Task OnGetAsync(Guid memberId, CancellationToken cancellationToken)
    {
        MemberId = memberId;

        // Execute three related API calls in parallel
        var membersTask = apiClient.GetMembersAsync(cancellationToken);
        var checkInsTask = apiClient.GetMemberCheckInsAsync(memberId, cancellationToken);
        var classesTask = apiClient.GetMemberClassesAsync(memberId, cancellationToken);
        await Task.WhenAll(membersTask, checkInsTask, classesTask);

        var membersResult = await membersTask;
        var checkInsResult = await checkInsTask;
        var classesResult = await classesTask;

        // Check for API errors
        Error = membersResult.Error ?? checkInsResult.Error ?? classesResult.Error;

        if (Error is null)
        {
            // Find the specific member from the members list
            Member = membersResult.Value!.SingleOrDefault(member => member.MemberId == memberId);

            if (Member is null)
            {
                // Member not found - create a client-side error
                Error = new ApiError(
                    "Member.NotFound",
                    "The member was not found.",
                    null);
                return;
            }

            CheckIns = checkInsResult.Value!;
            Classes = classesResult.Value!;
        }
    }

    /// <summary>
    /// Handles POST requests to check in a member.
    /// Demonstrates: 
    /// - POST-Redirect-GET pattern (prevents duplicate form submissions)
    /// - Business rule enforcement (API rejects inactive members)
    /// - TempData for cross-request messages
    /// </summary>
    public async Task<IActionResult> OnPostCheckInAsync(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.CheckInMemberAsync(memberId, cancellationToken);
        SetResultMessage(result, "Member checked in successfully.");

        // Redirect back to the GET handler to refresh the page
        return RedirectToPage(new { memberId });
    }

    /// <summary>
    /// Handles POST requests to upload a member document.
    /// Demonstrates:
    /// - File upload with multipart form-data
    /// - Client-side validation
    /// - Infrastructure-independent storage (opaque storage key)
    /// </summary>
    public async Task<IActionResult> OnPostUploadAsync(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        // Validate that a file was selected
        if (Document is null)
        {
            TempData["Error"] = "Choose a document before uploading.";
            return RedirectToPage(new { memberId });
        }

        // Open the file stream and upload to the API
        await using var content = Document.OpenReadStream();
        var contentType = string.IsNullOrWhiteSpace(Document.ContentType)
            ? "application/octet-stream"
            : Document.ContentType;

        var result = await apiClient.UploadMemberDocumentAsync(
            memberId,
            Document.FileName,
            contentType,
            content,
            cancellationToken);

        SetResultMessage(result, result.IsSuccess
            ? $"Document stored as {result.Value!.StorageKey}."
            : string.Empty);

        return RedirectToPage(new { memberId });
    }

    /// <summary>
    /// Helper method to set success or error messages in TempData.
    /// TempData persists across the redirect and is displayed in the layout.
    /// </summary>
    private void SetResultMessage<T>(ApiResult<T> result, string successMessage)
    {
        if (result.IsSuccess)
        {
            TempData["Message"] = successMessage;
            return;
        }

        TempData["Error"] = FormatError(result.Error!);
    }

    /// <summary>
    /// Formats an API error for display, including the operation ID if available.
    /// The operation ID can be used to correlate with server-side logs.
    /// </summary>
    private static string FormatError(ApiError error) =>
        string.IsNullOrWhiteSpace(error.OperationId)
            ? error.Message
            : $"{error.Message} Reference: {error.OperationId}";
}
