using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo.Repositories;

public sealed class MongoCheckInRepository : ICheckInRepository
{
    private readonly IMongoCollection<CheckIn> _checkIns;

    public MongoCheckInRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _checkIns = database.GetCollection<CheckIn>("CheckIns");
    }

    public async Task<IReadOnlyList<CheckIn>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<CheckIn>.Filter.Eq(c => c.MemberId, memberId);
        return await _checkIns
            .Find(filter)
            .SortByDescending(c => c.CheckInDate)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(
        CheckIn checkIn,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);
        await _checkIns.InsertOneAsync(checkIn, cancellationToken: cancellationToken);
    }
}
