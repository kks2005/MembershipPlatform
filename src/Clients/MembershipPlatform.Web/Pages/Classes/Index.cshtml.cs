using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages.Classes;

public sealed class IndexModel(IMembershipApiClient apiClient) : PageModel
{
    private Dictionary<Guid, ClassRegistrationSummary> summaries = [];

    public IReadOnlyList<FitnessClass> Classes { get; private set; } = [];

    public ApiError? Error { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var classesTask = apiClient.GetClassesAsync(cancellationToken);
        var summaryTask = apiClient.GetRegistrationSummaryAsync(cancellationToken);
        await Task.WhenAll(classesTask, summaryTask);

        var classesResult = await classesTask;
        var summaryResult = await summaryTask;
        Error = classesResult.Error ?? summaryResult.Error;

        if (Error is null)
        {
            Classes = classesResult.Value!
                .OrderBy(fitnessClass => fitnessClass.StartTime)
                .ToArray();
            summaries = summaryResult.Value!
                .ToDictionary(summary => summary.ClassId);
        }
    }

    public int GetRegistrationCount(Guid classId) =>
        summaries.TryGetValue(classId, out var summary)
            ? summary.RegistrationCount
            : 0;
}
