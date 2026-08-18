using MembershipPlatform.Core.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace MembershipPlatform.Storage.Local;

public static class DependencyInjection
{
    public static IServiceCollection AddLocalMemberDocumentStorage(
        this IServiceCollection services,
        string rootPath)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootPath);

        services.AddSingleton<IMemberDocumentStorage>(
            new LocalMemberDocumentStorage(rootPath));

        return services;
    }
}
