using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite.Repositories;

public sealed class SqliteMemberRepository(string connectionString) : IMemberRepository
{
    private readonly string connectionString = SqliteConnectionSettings.Normalize(connectionString);

    public async Task<IReadOnlyList<Member>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT MemberId, Name, Email, Status, JoinDate
            FROM Members
            ORDER BY Name;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
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
            FROM Members
            WHERE MemberId = $memberId;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$memberId", SqliteValue.From(memberId));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return ReadMember(reader);
    }

    private static Member ReadMember(SqliteDataReader reader) =>
        new()
        {
            MemberId = SqliteValue.ToGuid(reader.GetString(0)),
            Name = reader.GetString(1),
            Email = reader.GetString(2),
            Status = (MemberStatus)reader.GetInt32(3),
            JoinDate = SqliteValue.ToDateTimeOffset(reader.GetString(4))
        };
}
