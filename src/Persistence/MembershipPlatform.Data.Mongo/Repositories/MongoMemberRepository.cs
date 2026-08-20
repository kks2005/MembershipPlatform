using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo.Repositories;

public sealed class MongoMemberRepository : IMemberRepository
{
    private readonly IMongoCollection<Member> _members;

    public MongoMemberRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _members = database.GetCollection<Member>("Members");
    }

    public async Task<IReadOnlyList<Member>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _members
            .Find(FilterDefinition<Member>.Empty)
            .SortBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Member?> GetByIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Member>.Filter.Eq(m => m.MemberId, memberId);
        return await _members
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
