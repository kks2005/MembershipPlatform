using MembershipPlatform.Data.Sqlite.Queries;

namespace MembershipPlatform.Data.Sqlite.Tests.Queries;

[TestClass]
public sealed class SqliteClassRegistrationQueryTests
{
    [TestMethod]
    public async Task GivenSeededRegistrationsWhenGettingClassMembersThenReturnsRegisteredMembers()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);
        var classId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var query = new SqliteClassRegistrationQuery(database.ConnectionString);

        // Act
        var result = await query.GetMembersForClassAsync(classId);

        // Assert
        Assert.HasCount(2, result);
        Assert.IsTrue(result.Any(member => member.Name == "Active Member"));
        Assert.IsTrue(result.Any(member => member.Name == "Jordan Brooks"));
    }

    [TestMethod]
    public async Task GivenClassesWithAndWithoutRegistrationsWhenGettingSummaryThenReturnsBoth()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);
        var query = new SqliteClassRegistrationQuery(database.ConnectionString);

        // Act
        var result = await query.GetClassRegistrationSummaryAsync();

        // Assert
        Assert.HasCount(8, result);
        Assert.AreEqual(
            2,
            result.Single(summary => summary.ClassName == "Morning Yoga").RegistrationCount);
        Assert.AreEqual(
            2,
            result.Single(summary => summary.ClassName == "Strength Training").RegistrationCount);
        Assert.AreEqual(
            0,
            result.Single(summary => summary.ClassName == "Recovery Stretch").RegistrationCount);
    }
}
