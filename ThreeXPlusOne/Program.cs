using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne;
using ThreeXPlusOne.Cli;
using ThreeXPlusOne.Cli.Models;

CommandExecutionSettings commandExecutionSettings = CommandLineParser.ParseCommand(args);

using IHost host = Host.CreateDefaultBuilder(args)
                       .ConfigureApplication(commandExecutionSettings)
                       .Build();

using IServiceScope scope = host.Services.CreateScope();

CommandLineRunner clr = scope.ServiceProvider.GetRequiredService<CommandLineRunner>();

clr.RunCommand(commandExecutionSettings);