using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Members;

public sealed class DetailsModel(IMembershipApiClient apiClient) : PageModel
{
    public Guid MemberId { get; private set; }

    public MemberSummary? Member { get; private set; }

    public IReadOnlyList<MemberCheckIn> CheckIns { get; private set; } = [];

    public IReadOnlyList<FitnessClass> Classes { get; private set; } = [];

    public ApiError? Error { get; private set; }

    [BindProperty]
    public IFormFile? Document { get; set; }

    public async Task OnGetAsync(Guid memberId, CancellationToken cancellationToken)
    {
        MemberId = memberId;
        var membersTask = apiClient.GetMembersAsync(cancellationToken);
        var checkInsTask = apiClient.GetMemberCheckInsAsync(memberId, cancellationToken);
        var classesTask = apiClient.GetMemberClassesAsync(memberId, cancellationToken);
        await Task.WhenAll(membersTask, checkInsTask, classesTask);

        var membersResult = await membersTask;
        var checkInsResult = await checkInsTask;
        var classesResult = await classesTask;
        Error = membersResult.Error ?? checkInsResult.Error ?? classesResult.Error;

        if (Error is null)
        {
            Member = membersResult.Value!.SingleOrDefault(member => member.MemberId == memberId);

            if (Member is null)
            {
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

    public async Task<IActionResult> OnPostCheckInAsync(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        var result = await apiClient.CheckInMemberAsync(memberId, cancellationToken);
        SetResultMessage(result, "Member checked in successfully.");
        return RedirectToPage(new { memberId });
    }

    public async Task<IActionResult> OnPostUploadAsync(
        Guid memberId,
        CancellationToken cancellationToken)
    {
        if (Document is null)
        {
            TempData["Error"] = "Choose a document before uploading.";
            return RedirectToPage(new { memberId });
        }

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

    private void SetResultMessage<T>(ApiResult<T> result, string successMessage)
    {
        if (result.IsSuccess)
        {
            TempData["Message"] = successMessage;
            return;
        }

        TempData["Error"] = FormatError(result.Error!);
    }

    private static string FormatError(ApiError error) =>
        string.IsNullOrWhiteSpace(error.OperationId)
            ? error.Message
            : $"{error.Message} Reference: {error.OperationId}";
}
