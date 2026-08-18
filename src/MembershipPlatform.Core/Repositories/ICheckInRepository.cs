using MembershipPlatform.Core.Entities;

namespace MembershipPlatform.Core.Repositories;

public interface ICheckInRepository
{
    Task<IReadOnlyList<CheckIn>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default);

    Task AddAsync(CheckIn checkIn, CancellationToken cancellationToken = default);
}
