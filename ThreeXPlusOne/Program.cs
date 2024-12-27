using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ThreeXPlusOne;
using ThreeXPlusOne.CommandLine;
using ThreeXPlusOne.CommandLine.Models;
using ThreeXPlusOne.CommandLine.Services;
using ThreeXPlusOne.Logging;

Log.Logger = new LoggerConfiguration()
            .Enrich.With(new UserNameEnricher())
            .WriteTo.File(path: "Logging/logs/ThreeXPlusOneLogs-.txt",
                          outputTemplate: "[{UserName}] {Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                          rollingInterval: RollingInterval.Day,
                          rollOnFileSizeLimit: true,
                          retainedFileCountLimit: null)
            .CreateLogger();

Log.Information("Starting up ThreeXPlusOne app");

try
{
    CommandExecutionSettings commandExecutionSettings = CommandLineParser.ParseCommand(args);

    using IHost host = Host.CreateDefaultBuilder(args)
                           .UseSerilog()
                           .ConfigureApplication(commandExecutionSettings)
                           .Build();

    // Initialize settings before running the host
    var settingsService = host.Services.GetRequiredService<CommandExecutionSettingsService>();
    settingsService.Initialize(commandExecutionSettings);

    await host.RunAsync();
}
catch (PlatformNotSupportedException ex)
{
    Log.Fatal(ex, "ThreeXPlusOne app failed to start due to unsupported platform");
    Console.WriteLine($"\n{ex.Message}\n");
}
catch (Exception ex)
{
    Log.Fatal(ex, "ThreeXPlusOne app failed to start");
}
finally
{
    Log.Information("Shutting down ThreeXPlusOne app");
    Log.CloseAndFlush();
}