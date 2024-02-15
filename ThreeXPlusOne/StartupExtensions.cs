using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne.CommandLine;
using ThreeXPlusOne.App;
using ThreeXPlusOne.App.DirectedGraph;
using ThreeXPlusOne.App.Services;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Services.SkiaSharp;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.CommandLine.Models;
using ThreeXPlusOne.App.Models.Shapes;
using ThreeXPlusOne.App.Factories;

namespace ThreeXPlusOne;

public static class StartupExtensions
{
    /// <summary>
    /// Set up the host required for dependency injection
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
    /// Configure all required services for dependency injection
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    private static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<AppSettings>(configuration);

        services.AddScoped<CommandLineRunner>();

        services.AddScoped<IProcess, Process>();
        services.AddScoped<IAlgorithm, Algorithm>();
        services.AddScoped<IHistogram, Histogram>();
        services.AddScoped<IMetadata, Metadata>();
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IHistogramService, SkiaSharpHistogramService>();
        services.AddScoped<ILightSourceService, LightSourceService>();

        services.AddScoped<IDirectedGraph, TwoDimensionalDirectedGraph>();
        services.AddScoped<IDirectedGraph, ThreeDimensionalDirectedGraph>();
        services.AddScoped<IDirectedGraphService, SkiaSharpDirectedGraphService>();

        services.AddScoped<IShapeFactory, ShapeFactory>();

        services.AddScoped<IShape, Ellipse>();
        services.AddScoped<IShape, Polygon>();

        services.AddSingleton<IConsoleService, ConsoleService>();

        return services;
    }
}
