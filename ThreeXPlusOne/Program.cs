using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne;
using ThreeXPlusOne.CommandLine;
using ThreeXPlusOne.CommandLine.Models;
using ThreeXPlusOne.Logging;
using Serilog;

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
                           .ConfigureApplication(commandExecutionSettings)
                           .Build();

    using IServiceScope scope = host.Services.CreateScope();

    CommandLineRunner commandLineRunner = scope.ServiceProvider.GetRequiredService<CommandLineRunner>();

    await commandLineRunner.RunCommand(commandExecutionSettings);
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