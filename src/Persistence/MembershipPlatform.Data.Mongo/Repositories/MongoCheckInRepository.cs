using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Data.Mongo.Repositories;

public sealed class MongoCheckInRepository : ICheckInRepository
{
    public Task<IReadOnlyList<CheckIn>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement member check-in retrieval with the selected MongoDB driver.
        throw new NotImplementedException();
    }

    public Task AddAsync(CheckIn checkIn, CancellationToken cancellationToken = default)
    {
        // TODO: Implement check-in persistence with the selected MongoDB driver.
        throw new NotImplementedException();
    }
}
