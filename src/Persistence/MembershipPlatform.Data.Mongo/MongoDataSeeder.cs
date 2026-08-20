using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo;

public static class MongoDataSeeder
{
    public static async Task SeedAsync(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);

        var members = database.GetCollection<Member>("Members");
        var classes = database.GetCollection<FitnessClass>("Classes");
        var checkIns = database.GetCollection<CheckIn>("CheckIns");
        var registrations = database.GetCollection<ClassRegistration>("ClassRegistrations");

        // Clear existing data
        await members.DeleteManyAsync(FilterDefinition<Member>.Empty);
        await classes.DeleteManyAsync(FilterDefinition<FitnessClass>.Empty);
        await checkIns.DeleteManyAsync(FilterDefinition<CheckIn>.Empty);
        await registrations.DeleteManyAsync(FilterDefinition<ClassRegistration>.Empty);

        // Seed Members
        var activeMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var inactiveMemberId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        await members.InsertManyAsync(new[]
        {
            new Member
            {
                MemberId = activeMemberId,
                Name = "Active Member",
                Email = "active.member@example.com",
                Status = MemberStatus.Active,
                JoinDate = DateTimeOffset.UtcNow.AddMonths(-6)
            },
            new Member
            {
                MemberId = inactiveMemberId,
                Name = "Inactive Member",
                Email = "inactive.member@example.com",
                Status = MemberStatus.Inactive,
                JoinDate = DateTimeOffset.UtcNow.AddMonths(-12)
            }
        });

        // Seed Classes
        var yogaClassId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var strengthClassId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        await classes.InsertManyAsync(new[]
        {
            new FitnessClass
            {
                ClassId = yogaClassId,
                Name = "Morning Yoga",
                StartTime = DateTimeOffset.UtcNow.AddDays(1),
                Capacity = 20
            },
            new FitnessClass
            {
                ClassId = strengthClassId,
                Name = "Strength Training",
                StartTime = DateTimeOffset.UtcNow.AddDays(2),
                Capacity = 15
            }
        });

        // Seed CheckIns
        await checkIns.InsertManyAsync(new[]
        {
            new CheckIn
            {
                CheckInId = Guid.NewGuid(),
                MemberId = activeMemberId,
                CheckInDate = DateTimeOffset.UtcNow.AddHours(-2)
            },
            new CheckIn
            {
                CheckInId = Guid.NewGuid(),
                MemberId = activeMemberId,
                CheckInDate = DateTimeOffset.UtcNow.AddDays(-1)
            }
        });

        // Seed ClassRegistrations
        await registrations.InsertManyAsync(new[]
        {
            new ClassRegistration
            {
                RegistrationId = Guid.NewGuid(),
                ClassId = strengthClassId,
                MemberId = activeMemberId,
                RegisteredDate = DateTimeOffset.UtcNow.AddHours(-1),
                Status = RegistrationStatus.Registered
            }
        });

        // Create indexes for performance
        await CreateIndexesAsync(members, classes, checkIns, registrations);
    }

    private static async Task CreateIndexesAsync(
        IMongoCollection<Member> members,
        IMongoCollection<FitnessClass> classes,
        IMongoCollection<CheckIn> checkIns,
        IMongoCollection<ClassRegistration> registrations)
    {
        // Member indexes
        await members.Indexes.CreateOneAsync(
            new CreateIndexModel<Member>(
                Builders<Member>.IndexKeys.Ascending(m => m.Email),
                new CreateIndexOptions { Unique = true }
            )
        );

        // CheckIn indexes
        await checkIns.Indexes.CreateOneAsync(
            new CreateIndexModel<CheckIn>(
                Builders<CheckIn>.IndexKeys.Ascending(c => c.MemberId)
            )
        );

        // ClassRegistration indexes
        await registrations.Indexes.CreateOneAsync(
            new CreateIndexModel<ClassRegistration>(
                Builders<ClassRegistration>.IndexKeys
                    .Ascending(r => r.ClassId)
                    .Ascending(r => r.MemberId)
                    .Ascending(r => r.Status),
                new CreateIndexOptions { Unique = true }
            )
        );

        await registrations.Indexes.CreateOneAsync(
            new CreateIndexModel<ClassRegistration>(
                Builders<ClassRegistration>.IndexKeys.Ascending(r => r.MemberId)
            )
        );

        await registrations.Indexes.CreateOneAsync(
            new CreateIndexModel<ClassRegistration>(
                Builders<ClassRegistration>.IndexKeys.Ascending(r => r.ClassId)
            )
        );
    }
}
