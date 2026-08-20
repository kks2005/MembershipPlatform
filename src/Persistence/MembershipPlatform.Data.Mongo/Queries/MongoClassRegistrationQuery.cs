using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Queries;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo.Queries;

public sealed class MongoClassRegistrationQuery : IClassRegistrationQuery
{
    private readonly IMongoCollection<ClassRegistration> _registrations;
    private readonly IMongoCollection<Member> _members;
    private readonly IMongoCollection<FitnessClass> _classes;

    public MongoClassRegistrationQuery(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _registrations = database.GetCollection<ClassRegistration>("ClassRegistrations");
        _members = database.GetCollection<Member>("Members");
        _classes = database.GetCollection<FitnessClass>("Classes");
    }

    public async Task<IReadOnlyList<Member>> GetMembersForClassAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        // Find registrations for this class
        var registrationFilter = Builders<ClassRegistration>.Filter.And(
            Builders<ClassRegistration>.Filter.Eq(r => r.ClassId, classId),
            Builders<ClassRegistration>.Filter.Eq(r => r.Status, RegistrationStatus.Registered)
        );

        var registrations = await _registrations
            .Find(registrationFilter)
            .ToListAsync(cancellationToken);

        if (registrations.Count == 0)
        {
            return Array.Empty<Member>();
        }

        // Get member IDs
        var memberIds = registrations.Select(r => r.MemberId).ToList();

        // Find members
        var memberFilter = Builders<Member>.Filter.In(m => m.MemberId, memberIds);
        return await _members
            .Find(memberFilter)
            .SortBy(m => m.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClassRegistrationSummary>> GetClassRegistrationSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        // Get all classes
        var allClasses = await _classes
            .Find(FilterDefinition<FitnessClass>.Empty)
            .SortBy(c => c.StartTime)
            .ToListAsync(cancellationToken);

        // Get registration counts using aggregation
        var pipeline = _registrations.Aggregate()
            .Match(Builders<ClassRegistration>.Filter.Eq(r => r.Status, RegistrationStatus.Registered))
            .Group(
                r => r.ClassId,
                g => new { ClassId = g.Key, Count = g.Count() }
            );

        var registrationCounts = await pipeline.ToListAsync(cancellationToken);
        var countDictionary = registrationCounts.ToDictionary(x => x.ClassId, x => x.Count);

        // Combine results
        return allClasses
            .Select(c => new ClassRegistrationSummary(
                c.ClassId,
                c.Name,
                c.Capacity,
                countDictionary.TryGetValue(c.ClassId, out var count) ? count : 0
            ))
            .ToArray();
    }
}
