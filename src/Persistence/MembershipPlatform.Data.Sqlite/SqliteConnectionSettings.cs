using Microsoft.Data.Sqlite;

namespace MembershipPlatform.Data.Sqlite;

internal static class SqliteConnectionSettings
{
    public static string Normalize(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A connection string is required.", nameof(connectionString));
        }

        var builder = new SqliteConnectionStringBuilder(connectionString)
        {
            ForeignKeys = true
        };

        return builder.ToString();
    }
}
