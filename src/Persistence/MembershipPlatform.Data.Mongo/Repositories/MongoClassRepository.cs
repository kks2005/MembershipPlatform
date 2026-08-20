using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo.Repositories;

public sealed class MongoClassRepository : IClassRepository
{
    private readonly IMongoCollection<FitnessClass> _classes;

    public MongoClassRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _classes = database.GetCollection<FitnessClass>("Classes");
    }

    public async Task<IReadOnlyList<FitnessClass>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _classes
            .Find(FilterDefinition<FitnessClass>.Empty)
            .SortBy(c => c.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<FitnessClass?> GetByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<FitnessClass>.Filter.Eq(c => c.ClassId, classId);
        return await _classes
            .Find(filter)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
