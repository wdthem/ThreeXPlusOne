﻿using Microsoft.Extensions.Configuration;
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
using ThreeXPlusOne.Config;
using ThreeXPlusOne.CommandLine.Models;

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
                            string settingsFileFullPath = commandExecutionSettings.SettingsFileFullPath;

                            if (!string.IsNullOrEmpty(settingsFileFullPath))
                            {
                                configBuilder.AddJsonFile(settingsFileFullPath, optional: true, reloadOnChange: true);
                            }

                            if (string.IsNullOrEmpty(settingsFileFullPath))
                            {
                                settingsFileFullPath = commandExecutionSettings.SettingsFileName;
                            }

                            Dictionary<string, string?> inMemorySettings = new()
                                                            {
                                                                { nameof(Settings.SettingsFileName), commandExecutionSettings.SettingsFileName },
                                                                { nameof(Settings.SettingsFileFullPath), settingsFileFullPath }
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
        services.AddScoped<IFileService, FileService>();
        services.AddScoped<IHistogramService, SkiaSharpHistogramService>();
        services.AddScoped<ILightSourceService, LightSourceService>();

        services.AddScoped<IDirectedGraph, TwoDimensionalDirectedGraph>();
        services.AddScoped<IDirectedGraph, ThreeDimensionalDirectedGraph>();
        services.AddScoped<IDirectedGraphService, SkiaSharpDirectedGraphService>();

        services.AddSingleton<IConsoleService, ConsoleService>();

        return services;
    }
}
