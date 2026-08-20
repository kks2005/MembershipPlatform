using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite.Repositories;

public sealed class SqliteCheckInRepository(string connectionString) : ICheckInRepository
{
    private readonly string connectionString = SqliteConnectionSettings.Normalize(connectionString);

    public async Task<IReadOnlyList<CheckIn>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CheckInId, MemberId, CheckInDate
            FROM CheckIns
            WHERE MemberId = $memberId;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$memberId", SqliteValue.From(memberId));

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var checkIns = new List<CheckIn>();

        while (await reader.ReadAsync(cancellationToken))
        {
            checkIns.Add(new CheckIn
            {
                CheckInId = SqliteValue.ToGuid(reader.GetString(0)),
                MemberId = SqliteValue.ToGuid(reader.GetString(1)),
                CheckInDate = SqliteValue.ToDateTimeOffset(reader.GetString(2))
            });
        }

        return checkIns;
    }

    public async Task AddAsync(CheckIn checkIn, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);

        const string sql = """
            INSERT INTO CheckIns (CheckInId, MemberId, CheckInDate)
            VALUES ($checkInId, $memberId, $checkInDate);
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$checkInId", SqliteValue.From(checkIn.CheckInId));
        command.Parameters.AddWithValue("$memberId", SqliteValue.From(checkIn.MemberId));
        command.Parameters.AddWithValue("$checkInDate", SqliteValue.From(checkIn.CheckInDate));
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
