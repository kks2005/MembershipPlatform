using System.Globalization;

namespace MembershipPlatform.Data.Sqlite;

internal static class SqliteValue
{
    public static string From(Guid value) => value.ToString("D");

    public static string From(DateTimeOffset value) =>
        value.ToString("O", CultureInfo.InvariantCulture);

    public static Guid ToGuid(string value) => Guid.Parse(value);

    public static DateTimeOffset ToDateTimeOffset(string value) =>
        DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
}
