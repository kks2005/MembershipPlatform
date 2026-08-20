using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite;

public static class SqliteDatabaseInitializer
{
    public static async Task InitializeAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS Members
            (
                MemberId TEXT NOT NULL PRIMARY KEY,
                Name TEXT NOT NULL,
                Email TEXT NOT NULL,
                Status INTEGER NOT NULL,
                JoinDate TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS CheckIns
            (
                CheckInId TEXT NOT NULL PRIMARY KEY,
                MemberId TEXT NOT NULL,
                CheckInDate TEXT NOT NULL,
                FOREIGN KEY (MemberId) REFERENCES Members (MemberId)
            );

            CREATE TABLE IF NOT EXISTS Classes
            (
                ClassId TEXT NOT NULL PRIMARY KEY,
                Name TEXT NOT NULL,
                StartTime TEXT NOT NULL,
                Capacity INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ClassRegistrations
            (
                RegistrationId TEXT NOT NULL PRIMARY KEY,
                ClassId TEXT NOT NULL,
                MemberId TEXT NOT NULL,
                RegisteredDate TEXT NOT NULL,
                Status INTEGER NOT NULL,
                FOREIGN KEY (ClassId) REFERENCES Classes (ClassId),
                FOREIGN KEY (MemberId) REFERENCES Members (MemberId)
            );

            CREATE UNIQUE INDEX IF NOT EXISTS UX_ClassRegistrations_ActiveMemberClass
            ON ClassRegistrations (MemberId, ClassId)
            WHERE Status = 0;
            """;

        await using var connection = new SqliteConnection(
            SqliteConnectionSettings.Normalize(connectionString));
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
