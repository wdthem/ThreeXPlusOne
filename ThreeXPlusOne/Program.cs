using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne;
using ThreeXPlusOne.Cli;

using IHost host = Host.CreateDefaultBuilder(args)
                       .ConfigureApplication()
                       .Build();

using IServiceScope scope = host.Services.CreateScope();

CommandLineInterface cli = scope.ServiceProvider.GetRequiredService<CommandLineInterface>();

cli.RunCommand(args);