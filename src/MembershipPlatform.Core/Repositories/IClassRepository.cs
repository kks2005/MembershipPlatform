using MembershipPlatform.Core.Entities;

namespace MembershipPlatform.Core.Repositories;

public interface IClassRepository
{
    Task<IReadOnlyList<FitnessClass>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<FitnessClass?> GetByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default);
}
