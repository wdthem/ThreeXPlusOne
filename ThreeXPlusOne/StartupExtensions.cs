using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne.Cli;
using ThreeXPlusOne.App;
using ThreeXPlusOne.App.Graph;
using ThreeXPlusOne.App.Services;
using ThreeXPlusOne.App.Interfaces.Graph;
using ThreeXPlusOne.App.Interfaces.Helpers;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Services.SkiaSharp;
using ThreeXPlusOne.App.Helpers;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne;

public static class StartupExtensions
{
    private static readonly string _settingsFileName = "appSettings.json";

    /// <summary>
    /// Set up the host required for dependency injection
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="settingsFilePath"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureApplication(this IHostBuilder builder,
                                                    string settingsFilePath)
    {
        return builder.ConfigureAppConfiguration((context, configBuilder) =>
                        {
                            if (!string.IsNullOrEmpty(settingsFilePath))
                            {
                                configBuilder.AddJsonFile(settingsFilePath, optional: true, reloadOnChange: true);
                            }

                            if (string.IsNullOrEmpty(settingsFilePath))
                            {
                                settingsFilePath = _settingsFileName;
                            }

                            Dictionary<string, string?> inMemorySettings = new()
                                                            {
                                                                { "SettingsFilePath", settingsFilePath }
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
        services.Configure<Settings>(configuration);

        services.AddScoped<CommandLineRunner>();

        services.AddScoped<IProcess, Process>();
        services.AddScoped<IAlgorithm, Algorithm>();
        services.AddScoped<IHistogram, Histogram>();
        services.AddScoped<IMetadata, Metadata>();
        services.AddScoped<IFileHelper, FileHelper>();
        services.AddScoped<IHistogramService, SkiaSharpHistogramService>();
        services.AddScoped<ILightSourceService, LightSourceService>();

        services.AddScoped<IDirectedGraph, TwoDimensionalDirectedGraph>();
        services.AddScoped<IDirectedGraph, ThreeDimensionalDirectedGraph>();
        services.AddScoped<IDirectedGraphService, SkiaSharpDirectedGraphService>();

        services.AddSingleton<IConsoleHelper, ConsoleHelper>();

        return services;
    }
}