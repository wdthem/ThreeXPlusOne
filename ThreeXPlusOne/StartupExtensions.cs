using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Services;
using ThreeXPlusOne.Code.Helpers;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne;

public static class StartupExtensions
{
    private static readonly string _settingsFileName = "settings.json";

    /// <summary>
    /// Set up the host required for dependency injection
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureApplication(this IHostBuilder builder)
    {
        return builder.ConfigureAppConfiguration((context, configBuilder) =>
                            {
                                configBuilder.AddJsonFile(_settingsFileName, optional: true, reloadOnChange: true);
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