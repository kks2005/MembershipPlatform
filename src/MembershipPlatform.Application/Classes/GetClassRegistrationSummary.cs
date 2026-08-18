using MembershipPlatform.Core.Queries;

namespace MembershipPlatform.Application.Classes;

public sealed class GetClassRegistrationSummary(IClassRegistrationQuery classRegistrationQuery)
{
    public async Task<IReadOnlyList<GetClassRegistrationSummaryItem>> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var summaries = await classRegistrationQuery.GetClassRegistrationSummaryAsync(
            cancellationToken);

        return summaries
            .Select(summary => new GetClassRegistrationSummaryItem(
                summary.ClassId,
                summary.ClassName,
                summary.Capacity,
                summary.RegistrationCount))
            .ToArray();
    }
}
