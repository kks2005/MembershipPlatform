using MembershipPlatform.Core.Repositories;
using MembershipPlatform.Core.Queries;
using MembershipPlatform.Data.SqlServer.Queries;
using MembershipPlatform.Data.SqlServer.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace MembershipPlatform.Data.SqlServer;

public static class DependencyInjection
{
    public static IServiceCollection AddSqlServerPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("A connection string is required.", nameof(connectionString));
        }

        services.AddScoped<IMemberRepository>(_ =>
            new SqlServerMemberRepository(connectionString));
        services.AddScoped<ICheckInRepository>(_ =>
            new SqlServerCheckInRepository(connectionString));
        services.AddScoped<IClassRepository>(_ =>
            new SqlServerClassRepository(connectionString));
        services.AddScoped<IClassRegistrationRepository>(_ =>
            new SqlServerClassRegistrationRepository(connectionString));
        services.AddScoped<IMemberClassQuery>(_ =>
            new SqlServerMemberClassQuery(connectionString));
        services.AddScoped<IClassRegistrationQuery>(_ =>
            new SqlServerClassRegistrationQuery(connectionString));

        return services;
    }
}
