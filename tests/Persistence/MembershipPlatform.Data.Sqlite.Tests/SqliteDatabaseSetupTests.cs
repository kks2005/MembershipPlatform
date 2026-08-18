namespace MembershipPlatform.Data.Sqlite.Tests;

[TestClass]
public sealed class SqliteDatabaseSetupTests
{
    [TestMethod]
    public async Task GivenInitializedDatabaseWhenInitializingAgainThenFourTablesRemainAvailable()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();

        // Act
        await SqliteDatabaseInitializer.InitializeAsync(database.ConnectionString);

        // Assert
        var tableCount = await database.ExecuteScalarAsync(
            "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table';");
        Assert.AreEqual(4, tableCount);
    }

    [TestMethod]
    public async Task GivenSeededDatabaseWhenSeedingAgainThenSeedRowsAreNotDuplicated()
    {
        // Arrange
        await using var database = await SqliteTestDatabase.CreateAsync();
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);

        // Act
        await SqliteDataSeeder.SeedAsync(database.ConnectionString);

        // Assert
        Assert.AreEqual(8, await database.ExecuteScalarAsync("SELECT COUNT(*) FROM Members;"));
        Assert.AreEqual(8, await database.ExecuteScalarAsync("SELECT COUNT(*) FROM Classes;"));
        Assert.AreEqual(8, await database.ExecuteScalarAsync("SELECT COUNT(*) FROM CheckIns;"));
        Assert.AreEqual(
            12,
            await database.ExecuteScalarAsync("SELECT COUNT(*) FROM ClassRegistrations;"));
    }
}
