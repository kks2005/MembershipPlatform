using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Classes;

public sealed class DetailsModel(IMembershipApiClient apiClient) : PageModel
{
    public Guid ClassId { get; private set; }

    public FitnessClass? FitnessClass { get; private set; }

    public IReadOnlyList<ClassMember> Members { get; private set; } = [];

    public IReadOnlyList<MemberSummary> AvailableMembers { get; private set; } = [];

    public ApiError? Error { get; private set; }

    [BindProperty]
    public Guid MemberId { get; set; }

    public async Task OnGetAsync(Guid classId, CancellationToken cancellationToken)
    {
        ClassId = classId;
        var classesTask = apiClient.GetClassesAsync(cancellationToken);
        var classMembersTask = apiClient.GetClassMembersAsync(classId, cancellationToken);
        var availableMembersTask = apiClient.GetMembersAsync(cancellationToken);
        await Task.WhenAll(classesTask, classMembersTask, availableMembersTask);

        var classesResult = await classesTask;
        var classMembersResult = await classMembersTask;
        var availableMembersResult = await availableMembersTask;
        Error = classesResult.Error ?? classMembersResult.Error ?? availableMembersResult.Error;

        if (Error is null)
        {
            FitnessClass = classesResult.Value!.SingleOrDefault(item => item.ClassId == classId);
            Members = classMembersResult.Value!;
            AvailableMembers = availableMembersResult.Value!;

            var defaultMember = AvailableMembers.FirstOrDefault(member =>
                string.Equals(member.Status, "Active", StringComparison.OrdinalIgnoreCase))
                ?? (AvailableMembers.Count > 0 ? AvailableMembers[0] : null);
            MemberId = defaultMember?.MemberId ?? Guid.Empty;
        }
    }

    public async Task<IActionResult> OnPostRegisterAsync(
        Guid classId,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || MemberId == Guid.Empty)
        {
            TempData["Error"] = "Select a valid member.";
            return RedirectToPage(new { classId });
        }

        var result = await apiClient.RegisterMemberAsync(
            classId,
            MemberId,
            cancellationToken);

        if (result.IsSuccess)
        {
            TempData["Message"] = "Member registered successfully.";
        }
        else
        {
            TempData["Error"] = string.IsNullOrWhiteSpace(result.Error!.OperationId)
                ? result.Error.Message
                : $"{result.Error.Message} Reference: {result.Error.OperationId}";
        }

        return RedirectToPage(new { classId });
    }
}
