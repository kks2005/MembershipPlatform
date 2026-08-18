using System.Data;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Queries;
using Microsoft.Data.SqlClient;

namespace MembershipPlatform.Data.SqlServer.Queries;

public sealed class SqlServerClassRegistrationQuery(string connectionString)
    : IClassRegistrationQuery
{
    private readonly string connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("A connection string is required.", nameof(connectionString))
        : connectionString;

    public async Task<IReadOnlyList<Member>> GetMembersForClassAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT m.MemberId, m.Name, m.Email, m.Status, m.JoinDate
            FROM dbo.Members AS m
            INNER JOIN dbo.ClassRegistrations AS cr ON cr.MemberId = m.MemberId
            WHERE cr.ClassId = @ClassId
                AND cr.Status = @Status
            ORDER BY m.Name, m.MemberId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.UniqueIdentifier)
        {
            Value = classId
        });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int)
        {
            Value = (int)RegistrationStatus.Registered
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var members = new List<Member>();

        while (await reader.ReadAsync(cancellationToken))
        {
            members.Add(new Member
            {
                MemberId = reader.GetGuid(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Status = (MemberStatus)reader.GetInt32(3),
                JoinDate = reader.GetDateTimeOffset(4)
            });
        }

        return members;
    }

    public async Task<IReadOnlyList<ClassRegistrationSummary>>
        GetClassRegistrationSummaryAsync(CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT c.ClassId, c.Name, c.Capacity, COUNT(cr.RegistrationId)
            FROM dbo.Classes AS c
            LEFT JOIN dbo.ClassRegistrations AS cr
                ON cr.ClassId = c.ClassId
                AND cr.Status = @Status
            GROUP BY c.ClassId, c.Name, c.Capacity, c.StartTime
            ORDER BY c.StartTime, c.ClassId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int)
        {
            Value = (int)RegistrationStatus.Registered
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var summaries = new List<ClassRegistrationSummary>();

        while (await reader.ReadAsync(cancellationToken))
        {
            summaries.Add(new ClassRegistrationSummary(
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetInt32(3)));
        }

        return summaries;
    }
}
