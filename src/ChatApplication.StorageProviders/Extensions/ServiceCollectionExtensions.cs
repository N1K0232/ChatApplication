using ChatApplication.StorageProviders.Azure;
using ChatApplication.StorageProviders.FileSystem;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ChatApplication.StorageProviders.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, Action<AzureStorageSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var settings = new AzureStorageSettings();
        configuration.Invoke(settings);

        services.TryAddSingleton(settings);
        services.TryAddScoped<IStorageProvider, AzureStorageProvider>();

        return services;
    }

    public static IServiceCollection AddFileSystemStorage(this IServiceCollection services, Action<FileSystemStorageSettings> configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var settings = new FileSystemStorageSettings();
        configuration.Invoke(settings);

        services.TryAddSingleton(settings);
        services.TryAddScoped<IStorageProvider, FileSystemStorageProvider>();

        return services;
    }
}