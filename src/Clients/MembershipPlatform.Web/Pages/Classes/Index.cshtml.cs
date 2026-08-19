using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Classes;

/// <summary>
/// Page model for the fitness classes directory.
/// Demonstrates: Combining data from multiple endpoints to create a rich view.
/// </summary>
public sealed class IndexModel(IMembershipApiClient apiClient) : PageModel
{
    // Private field to store registration summaries for quick lookup
    private Dictionary<Guid, ClassRegistrationSummary> summaries = [];

    // Properties exposed to the Razor view
    public IReadOnlyList<FitnessClass> Classes { get; private set; } = [];
    public ApiError? Error { get; private set; }

    /// <summary>
    /// Handles GET requests to load all classes with their registration counts.
    /// Demonstrates:
    /// - Parallel API calls for performance
    /// - Combining data from multiple sources (classes + registration summaries)
    /// - Creating a dictionary for O(1) lookups in the view
    /// </summary>
    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // Execute two API calls in parallel
        var classesTask = apiClient.GetClassesAsync(cancellationToken);
        var summaryTask = apiClient.GetRegistrationSummaryAsync(cancellationToken);
        await Task.WhenAll(classesTask, summaryTask);

        var classesResult = await classesTask;
        var summaryResult = await summaryTask;

        // Check for errors from either API call
        Error = classesResult.Error ?? summaryResult.Error;

        if (Error is null)
        {
            // Sort classes by start time (soonest first)
            Classes = [.. classesResult.Value!.OrderBy(fitnessClass => fitnessClass.StartTime)];

            // Create a lookup dictionary for registration counts
            // This allows O(1) retrieval in GetRegistrationCount()
            summaries = summaryResult.Value!
                .ToDictionary(summary => summary.ClassId);
        }
    }

    /// <summary>
    /// Helper method to get registration count for a specific class.
    /// Called by the Razor view for each class card.
    /// Returns 0 if no registration data is available.
    /// </summary>
    public int GetRegistrationCount(Guid classId) =>
        summaries.TryGetValue(classId, out var summary)
            ? summary.RegistrationCount
            : 0;
}
