using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Queries;
using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite.Queries;

public sealed class SqliteMemberClassQuery(string connectionString) : IMemberClassQuery
{
    private readonly string connectionString = SqliteConnectionSettings.Normalize(connectionString);

    public async Task<IReadOnlyList<FitnessClass>> GetClassesForMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT c.ClassId, c.Name, c.StartTime, c.Capacity
            FROM Classes AS c
            INNER JOIN ClassRegistrations AS cr ON cr.ClassId = c.ClassId
            WHERE cr.MemberId = $memberId
                AND cr.Status = $status
            ORDER BY c.StartTime, c.ClassId;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$memberId", SqliteValue.From(memberId));
        command.Parameters.AddWithValue("$status", (int)RegistrationStatus.Registered);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var classes = new List<FitnessClass>();

        while (await reader.ReadAsync(cancellationToken))
        {
            classes.Add(new FitnessClass
            {
                ClassId = SqliteValue.ToGuid(reader.GetString(0)),
                Name = reader.GetString(1),
                StartTime = SqliteValue.ToDateTimeOffset(reader.GetString(2)),
                Capacity = reader.GetInt32(3)
            });
        }

        return classes;
    }
}
