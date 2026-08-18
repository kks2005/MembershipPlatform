using System.Data;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.SqlClient;

namespace MembershipPlatform.Data.SqlServer.Repositories;

public sealed class SqlServerClassRegistrationRepository(string connectionString)
    : IClassRegistrationRepository
{
    private readonly string connectionString = string.IsNullOrWhiteSpace(connectionString)
        ? throw new ArgumentException("A connection string is required.", nameof(connectionString))
        : connectionString;

    public async Task<bool> ExistsAsync(
        Guid memberId,
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS
            (
                SELECT 1
                FROM dbo.ClassRegistrations
                WHERE MemberId = @MemberId
                    AND ClassId = @ClassId
                    AND Status = @Status
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = new SqlCommand(sql, connection);
        AddClassAndStatusParameters(command, memberId, classId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return result is true;
    }

    public async Task<int> GetRegistrationCountAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM dbo.ClassRegistrations
            WHERE ClassId = @ClassId
                AND Status = @Status;
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

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result, System.Globalization.CultureInfo.InvariantCulture);
    }

    public async Task<ClassRegistrationAddResult> TryAddAsync(
        ClassRegistration registration,
        int classCapacity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);

        ArgumentOutOfRangeException.ThrowIfNegative(classCapacity);

        const string lockClassSql = """
            SELECT ClassId
            FROM dbo.Classes WITH (UPDLOCK, HOLDLOCK)
            WHERE ClassId = @ClassId;
            """;

        const string existsSql = """
            SELECT CASE WHEN EXISTS
            (
                SELECT 1
                FROM dbo.ClassRegistrations
                WHERE MemberId = @MemberId
                    AND ClassId = @ClassId
                    AND Status = @Status
            ) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END;
            """;

        const string countSql = """
            SELECT COUNT(*)
            FROM dbo.ClassRegistrations
            WHERE ClassId = @ClassId
                AND Status = @Status;
            """;

        const string insertSql = """
            INSERT INTO dbo.ClassRegistrations
                (RegistrationId, ClassId, MemberId, RegisteredDate, Status)
            VALUES
                (@RegistrationId, @ClassId, @MemberId, @RegisteredDate, @Status);
            """;

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        await using (var lockCommand = new SqlCommand(lockClassSql, connection, transaction))
        {
            lockCommand.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.UniqueIdentifier)
            {
                Value = registration.ClassId
            });
            await lockCommand.ExecuteScalarAsync(cancellationToken);
        }

        await using (var existsCommand = new SqlCommand(existsSql, connection, transaction))
        {
            AddClassAndStatusParameters(
                existsCommand,
                registration.MemberId,
                registration.ClassId);
            var exists = await existsCommand.ExecuteScalarAsync(cancellationToken);

            if (exists is true)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ClassRegistrationAddResult.AlreadyRegistered;
            }
        }

        await using (var countCommand = new SqlCommand(countSql, connection, transaction))
        {
            countCommand.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.UniqueIdentifier)
            {
                Value = registration.ClassId
            });
            countCommand.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int)
            {
                Value = (int)RegistrationStatus.Registered
            });
            var count = await countCommand.ExecuteScalarAsync(cancellationToken);

            if (Convert.ToInt32(count, System.Globalization.CultureInfo.InvariantCulture)
                >= classCapacity)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ClassRegistrationAddResult.AtCapacity;
            }
        }

        await using var command = new SqlCommand(insertSql, connection, transaction);
        command.Parameters.Add(new SqlParameter("@RegistrationId", SqlDbType.UniqueIdentifier)
        {
            Value = registration.RegistrationId
        });
        command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.UniqueIdentifier)
        {
            Value = registration.ClassId
        });
        command.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.UniqueIdentifier)
        {
            Value = registration.MemberId
        });
        command.Parameters.Add(new SqlParameter("@RegisteredDate", SqlDbType.DateTimeOffset)
        {
            Value = registration.RegisteredDate
        });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int)
        {
            Value = (int)registration.Status
        });

        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ClassRegistrationAddResult.Added;
    }

    private static void AddClassAndStatusParameters(
        SqlCommand command,
        Guid memberId,
        Guid classId)
    {
        command.Parameters.Add(new SqlParameter("@MemberId", SqlDbType.UniqueIdentifier)
        {
            Value = memberId
        });
        command.Parameters.Add(new SqlParameter("@ClassId", SqlDbType.UniqueIdentifier)
        {
            Value = classId
        });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.Int)
        {
            Value = (int)RegistrationStatus.Registered
        });
    }
}
