using MembershipPlatform.Core.Entities;

namespace MembershipPlatform.Core.Repositories;

public interface IMemberRepository
{
    Task<IReadOnlyList<Member>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<Member?> GetByIdAsync(Guid memberId, CancellationToken cancellationToken = default);
}
