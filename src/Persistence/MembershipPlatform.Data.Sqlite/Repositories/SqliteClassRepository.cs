using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite.Repositories;

public sealed class SqliteClassRepository(string connectionString) : IClassRepository
{
    private readonly string connectionString = SqliteConnectionSettings.Normalize(connectionString);

    public async Task<IReadOnlyList<FitnessClass>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ClassId, Name, StartTime, Capacity
            FROM Classes;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var classes = new List<FitnessClass>();

        while (await reader.ReadAsync(cancellationToken))
        {
            classes.Add(ReadClass(reader));
        }

        return classes;
    }

    public async Task<FitnessClass?> GetByIdAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ClassId, Name, StartTime, Capacity
            FROM Classes
            WHERE ClassId = $classId;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$classId", SqliteValue.From(classId));
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        return await reader.ReadAsync(cancellationToken)
            ? ReadClass(reader)
            : null;
    }

    private static FitnessClass ReadClass(SqliteDataReader reader) =>
        new()
        {
            ClassId = SqliteValue.ToGuid(reader.GetString(0)),
            Name = reader.GetString(1),
            StartTime = SqliteValue.ToDateTimeOffset(reader.GetString(2)),
            Capacity = reader.GetInt32(3)
        };
}
