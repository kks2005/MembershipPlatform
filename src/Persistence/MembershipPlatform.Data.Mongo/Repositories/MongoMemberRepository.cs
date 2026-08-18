using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;

namespace MembershipPlatform.Data.Mongo.Repositories;

public sealed class MongoMemberRepository : IMemberRepository
{
    public Task<IReadOnlyList<Member>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement member-list retrieval with the selected MongoDB driver.
        throw new NotImplementedException();
    }

    public Task<Member?> GetByIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement member retrieval with the selected MongoDB driver.
        throw new NotImplementedException();
    }
}
