using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne;
using ThreeXPlusOne.CommandLine;
using ThreeXPlusOne.CommandLine.Models;

CommandExecutionSettings commandExecutionSettings = CommandLineParser.ParseCommand(args);

using IHost host = Host.CreateDefaultBuilder(args)
                       .ConfigureApplication(commandExecutionSettings)
                       .Build();

using IServiceScope scope = host.Services.CreateScope();

CommandLineRunner commandLineRunner = scope.ServiceProvider.GetRequiredService<CommandLineRunner>();

await commandLineRunner.RunCommand(commandExecutionSettings);