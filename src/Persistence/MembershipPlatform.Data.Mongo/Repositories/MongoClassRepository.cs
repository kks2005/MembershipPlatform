using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Data.Mongo.Repositories;

public sealed class MongoClassRepository : IClassRepository
{
    public Task<IReadOnlyList<FitnessClass>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement class retrieval with the selected MongoDB driver.
        throw new NotImplementedException();
    }

    public Task<FitnessClass?> GetByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement class retrieval with the selected MongoDB driver.
        throw new NotImplementedException();
    }
}
