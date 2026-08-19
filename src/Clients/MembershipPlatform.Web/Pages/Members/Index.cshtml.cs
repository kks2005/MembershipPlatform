using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Members;

/// <summary>
/// Page model for the member directory with status filtering.
/// Demonstrates: Query string parameters, client-side filtering, and conditional rendering.
/// </summary>
public sealed class IndexModel(IMembershipApiClient apiClient) : PageModel
{
    // Properties exposed to the Razor view
    public IReadOnlyList<MemberSummary> Members { get; private set; } = [];
    public string SelectedStatus { get; private set; } = "All";
    public int TotalCount { get; private set; }
    public int ActiveCount { get; private set; }
    public int InactiveCount { get; private set; }
    public ApiError? Error { get; private set; }

    /// <summary>
    /// Handles GET requests with optional status filter via query string.
    /// Example URLs:
    /// - /Members - shows all members
    /// - /Members?status=Active - shows only active members
    /// - /Members?status=Inactive - shows only inactive members
    /// </summary>
    public async Task OnGetAsync(string? status, CancellationToken cancellationToken)
    {
        // Fetch all members from the API
        var result = await apiClient.GetMembersAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            Error = result.Error;
            return;
        }

        var members = result.Value!;

        // Calculate counts for all statuses (used in filter navigation)
        TotalCount = members.Length;
        ActiveCount = members.Count(IsActive);
        InactiveCount = members.Count(IsInactive);

        // Filter members based on the status query parameter
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

        // Default: show all members
        Members = members;
    }

    // Helper methods for filtering members by status
    private static bool IsActive(MemberSummary member) =>
        string.Equals(member.Status, "Active", StringComparison.OrdinalIgnoreCase);

    private static bool IsInactive(MemberSummary member) =>
        string.Equals(member.Status, "Inactive", StringComparison.OrdinalIgnoreCase);
}
