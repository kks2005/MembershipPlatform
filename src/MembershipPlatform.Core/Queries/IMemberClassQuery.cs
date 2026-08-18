using MembershipPlatform.Core.Entities;

namespace MembershipPlatform.Core.Queries;

public interface IMemberClassQuery
{
    Task<IReadOnlyList<FitnessClass>> GetClassesForMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken = default);
}
