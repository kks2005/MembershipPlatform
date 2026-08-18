using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using MembershipPlatform.Data.Sqlite.Repositories;

namespace MembershipPlatform.Data.Sqlite.Tests.Repositories;

[TestClass]
public sealed class SqliteClassRegistrationRepositoryTests
{
    private static readonly Guid ActiveMemberId =
        Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid InactiveMemberId =
        Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid EmptyClassId =
        Guid.Parse("c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6");

    [TestMethod]
    public async Task GivenConcurrentDuplicateRegistrationsWhenAddingThenOnlyOneIsAdded()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);
        var firstRepository = new SqliteClassRegistrationRepository(database.ConnectionString);
        var secondRepository = new SqliteClassRegistrationRepository(database.ConnectionString);
        var firstRegistration = CreateRegistration(ActiveMemberId);
        var secondRegistration = CreateRegistration(ActiveMemberId);

        // Act
        var results = await Task.WhenAll(
            firstRepository.TryAddAsync(firstRegistration, 20),
            secondRepository.TryAddAsync(secondRegistration, 20));

        // Assert
        CollectionAssert.AreEquivalent(
            new[]
            {
                ClassRegistrationAddResult.Added,
                ClassRegistrationAddResult.AlreadyRegistered
            },
            results);
        Assert.AreEqual(
            1,
            await database.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM ClassRegistrations WHERE ClassId = " +
                "'c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6' AND Status = 0;"));
    }

    [TestMethod]
    public async Task GivenOneRemainingPlaceWhenAddingConcurrentlyThenClassIsNotOverbooked()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);
        var firstRepository = new SqliteClassRegistrationRepository(database.ConnectionString);
        var secondRepository = new SqliteClassRegistrationRepository(database.ConnectionString);
        var firstRegistration = CreateRegistration(ActiveMemberId);
        var secondRegistration = CreateRegistration(InactiveMemberId);

        // Act
        var results = await Task.WhenAll(
            firstRepository.TryAddAsync(firstRegistration, 1),
            secondRepository.TryAddAsync(secondRegistration, 1));

        // Assert
        CollectionAssert.AreEquivalent(
            new[]
            {
                ClassRegistrationAddResult.Added,
                ClassRegistrationAddResult.AtCapacity
            },
            results);
        Assert.AreEqual(
            1,
            await database.ExecuteScalarAsync(
                "SELECT COUNT(*) FROM ClassRegistrations WHERE ClassId = " +
                "'c6c6c6c6-c6c6-c6c6-c6c6-c6c6c6c6c6c6' AND Status = 0;"));
    }

    private static ClassRegistration CreateRegistration(Guid memberId) =>
        new()
        {
            RegistrationId = Guid.NewGuid(),
            ClassId = EmptyClassId,
            MemberId = memberId,
            RegisteredDate = DateTimeOffset.UtcNow,
            Status = RegistrationStatus.Registered
        };
}
