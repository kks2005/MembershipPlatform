using MembershipPlatform.Core.Entities;

namespace MembershipPlatform.Core.Repositories;

public interface IClassRegistrationRepository
{
    Task<bool> ExistsAsync(
        Guid memberId,
        Guid classId,
        CancellationToken cancellationToken = default);

    Task<int> GetRegistrationCountAsync(
        Guid classId,
        CancellationToken cancellationToken = default);

    Task<ClassRegistrationAddResult> TryAddAsync(
        ClassRegistration registration,
        int classCapacity,
        CancellationToken cancellationToken = default);
}
