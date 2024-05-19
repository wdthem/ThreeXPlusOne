using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using ThreeXPlusOne.CommandLine;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.CommandLine.Models;

namespace ThreeXPlusOne;

public static class StartupExtensions
{
    /// <summary>
    /// Set up the host required for dependency injection and configure settings and the DI container
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="commandExecutionSettings"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureApplication(this IHostBuilder builder,
                                                    CommandExecutionSettings commandExecutionSettings)
    {
        return builder.ConfigureAppConfiguration((context, configBuilder) =>
                        {
                            string appSettingsFileFullPath = commandExecutionSettings.AppSettingsFileFullPath;

                            if (!string.IsNullOrEmpty(appSettingsFileFullPath))
                            {
                                configBuilder.AddJsonFile(appSettingsFileFullPath, optional: true, reloadOnChange: true);
                            }

                            if (string.IsNullOrEmpty(appSettingsFileFullPath))
                            {
                                appSettingsFileFullPath = commandExecutionSettings.AppSettingsFileName;
                            }

                            Dictionary<string, string?> inMemorySettings = new()
                                                            {
                                                                { nameof(AppSettings.SettingsFileName), commandExecutionSettings.AppSettingsFileName },
                                                                { nameof(AppSettings.SettingsFileFullPath), appSettingsFileFullPath }
                                                            };

                            configBuilder.AddInMemoryCollection(inMemorySettings);
                        })
                      .ConfigureServices((context, services) =>
                        {
                            services.AddServices(context.Configuration);
                        });
    }

    /// <summary>
    /// Configure all required classes/services for dependency injection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        //command line
        services.AddScoped<CommandLineRunner>();

        //app
        services.Configure<AppSettings>(configuration);
        services.AddScopedServices();
        services.AddSingletonServices();
        services.AddDirectedGraphs();
        services.AddDirectedGraphShapes();

        return services;
    }

    /// <summary>
    /// Add all implementers of IScopedService to the DI container
    /// </summary>
    /// <param name="services"></param>
    private static void AddScopedServices(this IServiceCollection services)
    {
        Assembly assembly = typeof(IScopedService).Assembly;

        List<Type> appServices = assembly.GetTypes()
                                         .Where(type => typeof(IScopedService).IsAssignableFrom(type) && !type.IsAbstract)
                                         .ToList();

        foreach (Type implementationType in appServices)
        {
            var interfaceTypes = implementationType.GetInterfaces()
                                                   .Where(i => i != typeof(IScopedService) && i != typeof(IDisposable))
                                                   .ToList();

            if (interfaceTypes.Count != 0)
            {
                foreach (Type interfaceType in interfaceTypes)
                {
                    services.AddScoped(interfaceType, implementationType);
                }
            }
            else
            {
                services.AddScoped(implementationType);
            }
        }
    }

    /// <summary>
    /// Add all implementers of ISingletonService to the DI container
    /// </summary>
    /// <param name="services"></param>
    private static void AddSingletonServices(this IServiceCollection services)
    {
        Assembly assembly = typeof(ISingletonService).Assembly;

        List<Type> appServices = assembly.GetTypes()
                                         .Where(type => typeof(ISingletonService).IsAssignableFrom(type) && !type.IsAbstract)
                                         .ToList();

        foreach (Type implementationType in appServices)
        {
            var interfaceTypes = implementationType.GetInterfaces()
                                                   .Where(i => i != typeof(ISingletonService) && i != typeof(IDisposable))
                                                   .ToList();

            if (interfaceTypes.Count != 0)
            {
                foreach (Type interfaceType in interfaceTypes)
                {
                    services.AddScoped(interfaceType, implementationType);
                }
            }
            else
            {
                services.AddScoped(implementationType);
            }
        }
    }

    /// <summary>
    /// Add all implementers of IDirectedGraph to the DI container
    /// </summary>
    /// <param name="services"></param>
    private static void AddDirectedGraphs(this IServiceCollection services)
    {
        Assembly assembly = typeof(IDirectedGraph).Assembly;

        List<Type> shapeTypes = assembly.GetTypes()
                                        .Where(type => typeof(IDirectedGraph).IsAssignableFrom(type) && !type.IsAbstract)
                                        .ToList();

        foreach (Type type in shapeTypes)
        {
            services.AddSingleton(typeof(IDirectedGraph), type);
        }
    }

    /// <summary>
    /// Add all implementers of IShape to the DI container
    /// </summary>
    /// <param name="services"></param>
    private static void AddDirectedGraphShapes(this IServiceCollection services)
    {
        Assembly assembly = typeof(IShape).Assembly;

        List<Type> shapeTypes = assembly.GetTypes()
                                        .Where(type => typeof(IShape).IsAssignableFrom(type) && !type.IsAbstract)
                                        .ToList();

        foreach (Type type in shapeTypes)
        {
            services.AddSingleton(typeof(IShape), type);
        }
    }
}