using MembershipPlatform.Data.Sqlite.Queries;

namespace MembershipPlatform.Data.Sqlite.Tests.Queries;

[TestClass]
public sealed class SqliteMemberClassQueryTests
{
    [TestMethod]
    public async Task GivenSeededRegistrationsWhenGettingMemberClassesThenReturnsRegisteredClasses()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);
        var memberId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var query = new SqliteMemberClassQuery(database.ConnectionString);

        // Act
        var result = await query.GetClassesForMemberAsync(memberId);

        // Assert
        Assert.HasCount(1, result);
        Assert.AreEqual("Strength Training", result[0].Name);
    }
}
