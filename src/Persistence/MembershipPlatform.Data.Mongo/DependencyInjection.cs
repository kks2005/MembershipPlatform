using MembershipPlatform.Core.Queries;
using MembershipPlatform.Core.Repositories;
using MembershipPlatform.Data.Mongo.Queries;
using MembershipPlatform.Data.Mongo.Repositories;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace MembershipPlatform.Data.Mongo;

public static class DependencyInjection
{
    public static IServiceCollection AddMongoDbPersistence(
        this IServiceCollection services,
        string connectionString,
        string databaseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(databaseName);

        // Register MongoDB client as singleton
        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        // Register database as scoped
        services.AddScoped<IMongoDatabase>(provider =>
        {
            var client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(databaseName);
        });

        // Register repositories
        services.AddScoped<IMemberRepository, MongoMemberRepository>();
        services.AddScoped<IClassRepository, MongoClassRepository>();
        services.AddScoped<ICheckInRepository, MongoCheckInRepository>();
        services.AddScoped<IClassRegistrationRepository, MongoClassRegistrationRepository>();

        // Register queries
        services.AddScoped<IMemberClassQuery, MongoMemberClassQuery>();
        services.AddScoped<IClassRegistrationQuery, MongoClassRegistrationQuery>();

        return services;
    }
}
