using System.Data;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Queries;
using Microsoft.Data.SqlClient;

namespace MembershipPlatform.Data.SqlServer.Queries;

public sealed class SqlServerMemberClassQuery(string connectionString) : IMemberClassQuery
{
    private readonly string connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("A connection string is required.", nameof(connectionString))
        : connectionString;

    public async Task<IReadOnlyList<FitnessClass>> GetClassesForMemberAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT c.ClassId, c.Name, c.StartTime, c.Capacity
            FROM dbo.Classes AS c
            INNER JOIN dbo.ClassRegistrations AS cr ON cr.ClassId = c.ClassId
            WHERE cr.MemberId = @MemberId
                AND cr.Status = @Status
            ORDER BY c.StartTime, c.ClassId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.UniqueIdentifier)
        {
            Value = memberId
        });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int)
        {
            Value = (int)RegistrationStatus.Registered
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var classes = new List<FitnessClass>();

        while (await reader.ReadAsync(cancellationToken))
        {
            classes.Add(new FitnessClass
            {
                ClassId = reader.GetGuid(0),
                Name = reader.GetString(1),
                StartTime = reader.GetDateTimeOffset(2),
                Capacity = reader.GetInt32(3)
            });
        }

        return classes;
    }
}
