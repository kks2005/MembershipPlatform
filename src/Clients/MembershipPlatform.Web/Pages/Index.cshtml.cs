using MembershipPlatform.Web.Api;
using MembershipPlatform.Web.Api.Contracts;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MembershipPlatform.Web.Pages;

public sealed class IndexModel(IMembershipApiClient apiClient) : PageModel
{
    private Dictionary<Guid, ClassRegistrationSummary> summaries = [];

    public IReadOnlyList<FitnessClass> UpcomingClasses { get; private set; } = [];

    public int MemberCount { get; private set; }

    public int ActiveMemberCount { get; private set; }

    public int InactiveMemberCount { get; private set; }

    public int ClassCount { get; private set; }

    public int RegistrationCount { get; private set; }

    public ApiError? Error { get; private set; }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        var membersTask = apiClient.GetMembersAsync(cancellationToken);
        var classesTask = apiClient.GetClassesAsync(cancellationToken);
        var summaryTask = apiClient.GetRegistrationSummaryAsync(cancellationToken);
        await Task.WhenAll(membersTask, classesTask, summaryTask);

        var membersResult = await membersTask;
        var classesResult = await classesTask;
        var summaryResult = await summaryTask;
        Error = membersResult.Error ?? classesResult.Error ?? summaryResult.Error;

        if (Error is not null)
        {
            return;
        }

        var members = membersResult.Value!;
        var classes = classesResult.Value!;
        var registrationSummaries = summaryResult.Value!;

        MemberCount = members.Length;
        ActiveMemberCount = members.Count(member =>
            string.Equals(member.Status, "Active", StringComparison.OrdinalIgnoreCase));
        InactiveMemberCount = members.Count(member =>
            string.Equals(member.Status, "Inactive", StringComparison.OrdinalIgnoreCase));
        ClassCount = classes.Length;
        RegistrationCount = registrationSummaries.Sum(summary => summary.RegistrationCount);
        UpcomingClasses = classes
            .OrderBy(fitnessClass => fitnessClass.StartTime)
            .Take(3)
            .ToArray();
        summaries = registrationSummaries.ToDictionary(summary => summary.ClassId);
    }

    public int GetRegistrationCount(Guid classId) =>
        summaries.TryGetValue(classId, out var summary)
            ? summary.RegistrationCount
            : 0;
}
