using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages;

/// <summary>
/// Page model for the dashboard (home page).
/// Demonstrates: Parallel API calls, data aggregation, and error handling.
/// </summary>
public sealed class IndexModel(IMembershipApiClient apiClient) : PageModel
{
    // Private field to store registration summaries for quick lookup
    private Dictionary<Guid, ClassRegistrationSummary> summaries = [];

    // Public properties exposed to the Razor view
    public IReadOnlyList<FitnessClass> UpcomingClasses { get; private set; } = [];
    public int MemberCount { get; private set; }
    public int ActiveMemberCount { get; private set; }
    public int InactiveMemberCount { get; private set; }
    public int ClassCount { get; private set; }
    public int RegistrationCount { get; private set; }
    public ApiError? Error { get; private set; }

    /// <summary>
    /// Handles GET requests to the dashboard page.
    /// Demonstrates:
    /// - Parallel API calls using Task.WhenAll (performance optimization)
    /// - Data aggregation and transformation
    /// - Error-first approach (check for errors before processing data)
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // Execute three API calls in parallel for better performance
        // This reduces total wait time compared to sequential calls
        var membersTask = apiClient.GetMembersAsync(cancellationToken);
        var classesTask = apiClient.GetClassesAsync(cancellationToken);
        var summaryTask = apiClient.GetRegistrationSummaryAsync(cancellationToken);
        await Task.WhenAll(membersTask, classesTask, summaryTask);

        // Await each task to get the results
        var membersResult = await membersTask;
        var classesResult = await classesTask;
        var summaryResult = await summaryTask;

        // Check for errors from any of the API calls
        // Use the first error encountered (error-first pattern)
        Error = membersResult.Error ?? classesResult.Error ?? summaryResult.Error;

        if (Error is not null)
        {
            // If any API call failed, stop processing and show error in the view
            return;
        }

        // Extract successful results (! asserts non-null since we checked Error above)
        var members = membersResult.Value!;
        var classes = classesResult.Value!;
        var registrationSummaries = summaryResult.Value!;

        // Calculate summary statistics for the dashboard cards
        MemberCount = members.Length;
        ActiveMemberCount = members.Count(member =>
            string.Equals(member.Status, "Active", StringComparison.OrdinalIgnoreCase));
        InactiveMemberCount = members.Count(member =>
            string.Equals(member.Status, "Inactive", StringComparison.OrdinalIgnoreCase));
        ClassCount = classes.Length;
        RegistrationCount = registrationSummaries.Sum(summary => summary.RegistrationCount);

        // Select the 3 most recent upcoming classes for preview
        UpcomingClasses = [.. classes
            .OrderBy(fitnessClass => fitnessClass.StartTime)
            .Take(3)];

        // Create a lookup dictionary for quick registration count retrieval
        summaries = registrationSummaries.ToDictionary(summary => summary.ClassId);
    }

    /// <summary>
    /// Helper method to get registration count for a specific class.
    /// Used by the Razor view when displaying each class card.
    /// </summary>
    public int GetRegistrationCount(Guid classId) =>
        summaries.TryGetValue(classId, out var summary)
            ? summary.RegistrationCount
            : 0;
}
