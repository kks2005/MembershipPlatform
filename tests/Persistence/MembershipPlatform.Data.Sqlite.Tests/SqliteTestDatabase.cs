using System.Globalization;
using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite.Tests;

internal sealed class SqliteTestDatabase : IAsyncDisposable
{
    private readonly string databasePath;

    private SqliteTestDatabase(string databasePath)
    {
        this.databasePath = databasePath;
        ConnectionString = $"Data Source={databasePath};Foreign Keys=True;Pooling=False";
    }

    public string ConnectionString { get; }

    public static async Task<SqliteTestDatabase> CreateAsync()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"membership-platform-{Guid.NewGuid():N}.db");
        var database = new SqliteTestDatabase(databasePath);
        await SqliteDatabaseInitializer.InitializeAsync(database.ConnectionString);
        return database;
    }

    public async Task<long> ExecuteScalarAsync(string sql)
    {
        await using var connection = new SqliteConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt64(result, CultureInfo.InvariantCulture);
    }

    public ValueTask DisposeAsync()
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        return ValueTask.CompletedTask;
    }
}
