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
    private static readonly string _settingsFileName = "settings.json";

    /// <summary>
    /// Set up the host required for dependency injection
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureApplication(this IHostBuilder builder)
    {
        return builder
                    //.ConfigureAppConfiguration((context, configBuilder) =>
                    //{
                    //    configBuilder.AddJsonFile(_settingsFileName, optional: true, reloadOnChange: true);
                    //})
                    .ConfigureServices((context, services) =>
                    {
                        services.AddServices(context.Configuration);
                    });
    }

    private static void BindSettings(this IHost host, string settingsPath)
    {
        var configuration = host.Services.GetRequiredService<IConfiguration>();
        var configurationRoot = configuration as IConfigurationRoot;
        var builder = new ConfigurationBuilder()
            .AddJsonFile(settingsPath, optional: false, reloadOnChange: true);

        if (configurationRoot != null)
        {
            foreach (var provider in configurationRoot.Providers.ToList())
            {
                provider.AddConfiguration(configuration);
            }
        }

        var newConfiguration = builder.Build();
        // Here, you have a couple of options:
        // 1. Update the DI container with the new configuration instance (complex and not recommended).
        // 2. Use the new configuration directly where needed (simpler, more practical in many cases).
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

        services.AddScoped<CommandLineInterface>();

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