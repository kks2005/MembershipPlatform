using MembershipPlatform.Core.Entities;
using MembershipPlatform.Data.Sqlite.Repositories;

namespace MembershipPlatform.Data.Sqlite.Tests.Repositories;

[TestClass]
public sealed class SqliteCheckInRepositoryTests
{
    [TestMethod]
    public async Task GivenValidCheckInWhenAddingAndReadingThenRoundTripsValues()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);
        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var checkIn = new CheckIn
        {
            CheckInId = Guid.NewGuid(),
            MemberId = memberId,
            CheckInDate = new DateTimeOffset(2026, 8, 17, 12, 30, 0, TimeSpan.Zero)
        };
        var repository = new SqliteCheckInRepository(database.ConnectionString);

        // Act
        await repository.AddAsync(checkIn);
        var result = await repository.GetByMemberIdAsync(memberId);

        // Assert
        var savedCheckIn = result.Single(item => item.CheckInId == checkIn.CheckInId);
        Assert.AreEqual(checkIn.MemberId, savedCheckIn.MemberId);
        Assert.AreEqual(checkIn.CheckInDate, savedCheckIn.CheckInDate);
    }
}
