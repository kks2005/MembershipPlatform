using System.Globalization;
using MembershipPlatform.Core.Entities;
using MembershipPlatform.Core.Enums;
using MembershipPlatform.Core.Repositories;
using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite.Repositories;

public sealed class SqliteClassRegistrationRepository(string connectionString)
    : IClassRegistrationRepository
{
    private readonly string connectionString = SqliteConnectionSettings.Normalize(connectionString);

    public async Task<bool> ExistsAsync(
        Guid memberId,
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM ClassRegistrations
                WHERE MemberId = $memberId
                    AND ClassId = $classId
                    AND Status = $status
            );
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        AddClassAndStatusParameters(command, memberId, classId);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result, CultureInfo.InvariantCulture) == 1;
    }

    public async Task<int> GetRegistrationCountAsync(
        Guid classId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT COUNT(*)
            FROM ClassRegistrations
            WHERE ClassId = $classId
                AND Status = $status;
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("$classId", SqliteValue.From(classId));
        command.Parameters.AddWithValue("$status", (int)RegistrationStatus.Registered);

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result, CultureInfo.InvariantCulture);
    }

    public async Task<ClassRegistrationAddResult> TryAddAsync(
        ClassRegistration registration,
        int classCapacity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registration);

        ArgumentOutOfRangeException.ThrowIfNegative(classCapacity);

        const string existsSql = """
            SELECT EXISTS
            (
                SELECT 1
                FROM ClassRegistrations
                WHERE MemberId = $memberId
                    AND ClassId = $classId
                    AND Status = $status
            );
            """;

        const string countSql = """
            SELECT COUNT(*)
            FROM ClassRegistrations
            WHERE ClassId = $classId
                AND Status = $status;
            """;

        const string insertSql = """
            INSERT INTO ClassRegistrations
                (RegistrationId, ClassId, MemberId, RegisteredDate, Status)
            VALUES
                ($registrationId, $classId, $memberId, $registeredDate, $status);
            """;

        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var transaction = connection.BeginTransaction(deferred: false);

        await using (var existsCommand = connection.CreateCommand())
        {
            existsCommand.Transaction = transaction;
            existsCommand.CommandText = existsSql;
            AddClassAndStatusParameters(
                existsCommand,
                registration.MemberId,
                registration.ClassId);

            var exists = await existsCommand.ExecuteScalarAsync(cancellationToken);
            if (Convert.ToInt64(exists, CultureInfo.InvariantCulture) == 1)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ClassRegistrationAddResult.AlreadyRegistered;
            }
        }

        await using (var countCommand = connection.CreateCommand())
        {
            countCommand.Transaction = transaction;
            countCommand.CommandText = countSql;
            countCommand.Parameters.AddWithValue(
                "$classId",
                SqliteValue.From(registration.ClassId));
            countCommand.Parameters.AddWithValue(
                "$status",
                (int)RegistrationStatus.Registered);

            var count = await countCommand.ExecuteScalarAsync(cancellationToken);
            if (Convert.ToInt32(count, CultureInfo.InvariantCulture) >= classCapacity)
            {
                await transaction.RollbackAsync(cancellationToken);
                return ClassRegistrationAddResult.AtCapacity;
            }
        }

        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = insertSql;
        command.Parameters.AddWithValue(
            "$registrationId",
            SqliteValue.From(registration.RegistrationId));
        command.Parameters.AddWithValue("$classId", SqliteValue.From(registration.ClassId));
        command.Parameters.AddWithValue("$memberId", SqliteValue.From(registration.MemberId));
        command.Parameters.AddWithValue(
            "$registeredDate",
            SqliteValue.From(registration.RegisteredDate));
        command.Parameters.AddWithValue("$status", (int)registration.Status);
        await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ClassRegistrationAddResult.Added;
    }

    private static void AddClassAndStatusParameters(
        SqliteCommand command,
        Guid memberId,
        Guid classId)
    {
        command.Parameters.AddWithValue("$memberId", SqliteValue.From(memberId));
        command.Parameters.AddWithValue("$classId", SqliteValue.From(classId));
        command.Parameters.AddWithValue("$status", (int)RegistrationStatus.Registered);
    }
}
