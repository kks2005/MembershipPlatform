using System.Data;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.SqlClient;

namespace MembershipPlatform.Data.SqlServer.Repositories;

public sealed class SqlServerCheckInRepository(string connectionString) : ICheckInRepository
{
    private readonly string connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("A connection string is required.", nameof(connectionString))
        : connectionString;

    public async Task<IReadOnlyList<CheckIn>> GetByMemberIdAsync(
        Guid memberId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CheckInId, MemberId, CheckInDate
            FROM dbo.CheckIns
            WHERE MemberId = @MemberId;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.UniqueIdentifier)
        {
            Value = memberId
        });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var checkIns = new List<CheckIn>();

        while (await reader.ReadAsync(cancellationToken))
        {
            checkIns.Add(new CheckIn
            {
                CheckInId = reader.GetGuid(0),
                MemberId = reader.GetGuid(1),
                CheckInDate = reader.GetDateTimeOffset(2)
            });
        }

        return checkIns;
    }

    public async Task AddAsync(CheckIn checkIn, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(checkIn);

        const string sql = """
            INSERT INTO dbo.CheckIns (CheckInId, MemberId, CheckInDate)
            VALUES (@CheckInId, @MemberId, @CheckInDate);
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@CheckInId", SqlDbType.UniqueIdentifier)
        {
            Value = checkIn.CheckInId
        });
        command.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.UniqueIdentifier)
        {
            Value = checkIn.MemberId
        });
        command.Parameters.Add(new SqlParameter("@CheckInDate", SqlDbType.DateTimeOffset)
        {
            Value = checkIn.CheckInDate
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
