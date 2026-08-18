using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Members;

public sealed class IndexModel(IMembershipApiClient apiClient) : PageModel
{
    public IReadOnlyList<MemberSummary> Members { get; private set; } = [];

    public string SelectedStatus { get; private set; } = "All";

    public int TotalCount { get; private set; }

    public int ActiveCount { get; private set; }

    public int InactiveCount { get; private set; }

    public ApiError? Error { get; private set; }

    public async Task OnGetAsync(string? status, CancellationToken cancellationToken)
    {
        var result = await apiClient.GetMembersAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            Error = result.Error;
            return;
        }

        var members = result.Value!;
        TotalCount = members.Length;
        ActiveCount = members.Count(IsActive);
        InactiveCount = members.Count(IsInactive);

        if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
        {
            SelectedStatus = "Active";
            Members = members.Where(IsActive).ToArray();
            return;
        }

        if (string.Equals(status, "Inactive", StringComparison.OrdinalIgnoreCase))
        {
            SelectedStatus = "Inactive";
            Members = members.Where(IsInactive).ToArray();
            return;
        }

        Members = members;
    }

    private static bool IsActive(MemberSummary member) =>
        string.Equals(member.Status, "Active", StringComparison.OrdinalIgnoreCase);

    private static bool IsInactive(MemberSummary member) =>
        string.Equals(member.Status, "Inactive", StringComparison.OrdinalIgnoreCase);
}
