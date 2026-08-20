using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo.Repositories;

public sealed class MongoClassRegistrationRepository : IClassRegistrationRepository
{
    private readonly IMongoCollection<ClassRegistration> _registrations;

    public MongoClassRegistrationRepository(IMongoDatabase database)
    {
        ArgumentNullException.ThrowIfNull(database);
        _registrations = database.GetCollection<ClassRegistration>("ClassRegistrations");
    }

    public async Task<bool> ExistsAsync(
        Guid memberId,
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ClassRegistration>.Filter.And(
            Builders<ClassRegistration>.Filter.Eq(r => r.MemberId, memberId),
            Builders<ClassRegistration>.Filter.Eq(r => r.ClassId, classId),
            Builders<ClassRegistration>.Filter.Eq(r => r.Status, RegistrationStatus.Registered)
        );

        var count = await _registrations.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<int> GetRegistrationCountAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<ClassRegistration>.Filter.And(
            Builders<ClassRegistration>.Filter.Eq(r => r.ClassId, classId),
            Builders<ClassRegistration>.Filter.Eq(r => r.Status, RegistrationStatus.Registered)
        );

        return (int)await _registrations.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
    }

    public async Task<ClassRegistrationAddResult> TryAddAsync(
        ClassRegistration registration,
        int classCapacity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);
        ArgumentOutOfRangeException.ThrowIfNegative(classCapacity);

        // MongoDB transactions require replica set or sharded cluster
        // For single-server setup, we do best-effort atomic check

        // Check if already registered
        if (await ExistsAsync(registration.MemberId, registration.ClassId, cancellationToken))
        {
            return ClassRegistrationAddResult.AlreadyRegistered;
        }

        // Check capacity
        var currentCount = await GetRegistrationCountAsync(registration.ClassId, cancellationToken);
        if (currentCount >= classCapacity)
        {
            return ClassRegistrationAddResult.AtCapacity;
        }

        // Try to insert
        try
        {
            await _registrations.InsertOneAsync(registration, cancellationToken: cancellationToken);
            return ClassRegistrationAddResult.Added;
        }
        catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
        {
            // Duplicate key - already registered (race condition)
            return ClassRegistrationAddResult.AlreadyRegistered;
        }
    }
}
