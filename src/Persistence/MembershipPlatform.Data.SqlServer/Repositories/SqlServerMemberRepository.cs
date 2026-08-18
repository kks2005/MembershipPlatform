using System.Data;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.SqlClient;

namespace MembershipPlatform.Data.SqlServer.Repositories;

public sealed class SqlServerMemberRepository(string connectionString) : IMemberRepository
{
    private readonly string connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("A connection string is required.", nameof(connectionString))
        : connectionString;

    public async Task<IReadOnlyList<Member>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT MemberId, Name, Email, Status, JoinDate
            FROM dbo.Members
            ORDER BY Name;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var members = new List<Member>();

        while (await reader.ReadAsync(cancellationToken))
        {
            members.Add(ReadMember(reader));
        }

        return members;
    }

    public async Task<Member?> GetByIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT MemberId, Name, Email, Status, JoinDate
            FROM dbo.Members
            WHERE MemberId = @MemberId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.UniqueIdentifier)
        {
            Value = memberId
        });

        await using var reader = await command.ExecuteReaderAsync(
            CommandBehavior.SingleRow,
            cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadMember(reader);
    }

    private static Member ReadMember(SqlDataReader reader) =>
        new()
        {
            MemberId = reader.GetGuid(0),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Status = (MemberStatus)reader.GetInt32(3),
            JoinDate = reader.GetDateTimeOffset(4)
        };
}
