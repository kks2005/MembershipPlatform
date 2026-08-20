using MembershipPlatform.Core.Repositories;
using MembershipPlatform.Core.Queries;
using MembershipPlatform.Data.Sqlite.Queries;
using MembershipPlatform.Data.Sqlite.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MembershipPlatform.Data.Sqlite;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlitePersistence(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        var normalizedConnectionString = SqliteConnectionSettings.Normalize(connectionString);

        services.AddScoped<IMemberRepository>(_ =>
            new SqliteMemberRepository(normalizedConnectionString));
        services.AddScoped<ICheckInRepository>(_ =>
            new SqliteCheckInRepository(normalizedConnectionString));
        services.AddScoped<IClassRepository>(_ =>
            new SqliteClassRepository(normalizedConnectionString));
        services.AddScoped<IClassRegistrationRepository>(_ =>
            new SqliteClassRegistrationRepository(normalizedConnectionString));
        services.AddScoped<IMemberClassQuery>(_ =>
            new SqliteMemberClassQuery(normalizedConnectionString));
        services.AddScoped<IClassRegistrationQuery>(_ =>
            new SqliteClassRegistrationQuery(normalizedConnectionString));

        return services;
    }
}
