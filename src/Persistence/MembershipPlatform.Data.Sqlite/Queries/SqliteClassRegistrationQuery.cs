using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Queries;
using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite.Queries;

public sealed class SqliteClassRegistrationQuery(string connectionString)
    : IClassRegistrationQuery
{
    private readonly string connectionString = SqliteConnectionSettings.Normalize(connectionString);

    public async Task<IReadOnlyList<Member>> GetMembersForClassAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT m.MemberId, m.Name, m.Email, m.Status, m.JoinDate
            FROM Members AS m
            INNER JOIN ClassRegistrations AS cr ON cr.MemberId = m.MemberId
            WHERE cr.ClassId = $classId
                AND cr.Status = $status
            ORDER BY m.Name, m.MemberId;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$classId", SqliteValue.From(classId));
        command.Parameters.AddWithValue("$status", (int)RegistrationStatus.Registered);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var members = new List<Member>();

        while (await reader.ReadAsync(cancellationToken))
        {
            members.Add(new Member
            {
                MemberId = SqliteValue.ToGuid(reader.GetString(0)),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Status = (MemberStatus)reader.GetInt32(3),
                JoinDate = SqliteValue.ToDateTimeOffset(reader.GetString(4))
            });
        }

        return members;
    }

    public async Task<IReadOnlyList<ClassRegistrationSummary>>
        GetClassRegistrationSummaryAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT c.ClassId, c.Name, c.Capacity, COUNT(cr.RegistrationId)
            FROM Classes AS c
            LEFT JOIN ClassRegistrations AS cr
                ON cr.ClassId = c.ClassId
                AND cr.Status = $status
            GROUP BY c.ClassId, c.Name, c.Capacity
            ORDER BY c.StartTime, c.ClassId;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$status", (int)RegistrationStatus.Registered);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var summaries = new List<ClassRegistrationSummary>();

        while (await reader.ReadAsync(cancellationToken))
        {
            summaries.Add(new ClassRegistrationSummary(
                SqliteValue.ToGuid(reader.GetString(0)),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3)));
        }

        return summaries;
    }
}
