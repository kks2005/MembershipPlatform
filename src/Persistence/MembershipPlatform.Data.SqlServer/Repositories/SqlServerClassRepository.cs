using System.Data;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.SqlClient;

namespace MembershipPlatform.Data.SqlServer.Repositories;

public sealed class SqlServerClassRepository(string connectionString) : IClassRepository
{
    private readonly string connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("A connection string is required.", nameof(connectionString))
        : connectionString;

    public async Task<IReadOnlyList<FitnessClass>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT ClassId, Name, StartTime, Capacity
            FROM dbo.Classes;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
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
            FROM dbo.Classes
            WHERE ClassId = @ClassId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.UniqueIdentifier)
        {
            Value = classId
        });

        await using var reader = await command.ExecuteReaderAsync(
            CommandBehavior.SingleRow,
            cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadClass(reader);
    }

    private static FitnessClass ReadClass(SqlDataReader reader) =>
        new()
        {
            ClassId = reader.GetGuid(0),
            Name = reader.GetString(1),
            StartTime = reader.GetDateTimeOffset(2),
            Capacity = reader.GetInt32(3)
        };
}
