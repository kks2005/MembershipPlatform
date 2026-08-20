using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Queries;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo.Queries;

public sealed class MongoMemberClassQuery : IMemberClassQuery
{
    private readonly IMongoCollection<ClassRegistration> _registrations;
    private readonly IMongoCollection<FitnessClass> _classes;

    public MongoMemberClassQuery(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _registrations = database.GetCollection<ClassRegistration>("ClassRegistrations");
        _classes = database.GetCollection<FitnessClass>("Classes");
    }

    public async Task<IReadOnlyList<FitnessClass>> GetClassesForMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        // Find registrations for this member
        var registrationFilter = Builders<ClassRegistration>.Filter.And(
            Builders<ClassRegistration>.Filter.Eq(r => r.MemberId, memberId),
            Builders<ClassRegistration>.Filter.Eq(r => r.Status, RegistrationStatus.Registered)
        );

        var registrations = await _registrations
            .Find(registrationFilter)
            .ToListAsync(cancellationToken);

        if (registrations.Count == 0)
        {
            return Array.Empty<FitnessClass>();
        }

        // Get class IDs
        var classIds = registrations.Select(r => r.ClassId).ToList();

        // Find classes
        var classFilter = Builders<FitnessClass>.Filter.In(c => c.ClassId, classIds);
        return await _classes
            .Find(classFilter)
            .SortBy(c => c.StartTime)
            .ToListAsync(cancellationToken);
    }
}
