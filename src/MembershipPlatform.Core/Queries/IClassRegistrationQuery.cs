using MembershipPlatform.Core.Entities;

namespace MembershipPlatform.Core.Queries;

public interface IClassRegistrationQuery
{
    Task<IReadOnlyList<Member>> GetMembersForClassAsync(
        Guid classId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClassRegistrationSummary>> GetClassRegistrationSummaryAsync(
        CancellationToken cancellationToken = default);
}
