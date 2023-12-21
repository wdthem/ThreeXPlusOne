using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne;
using ThreeXPlusOne.Code.Interfaces;

using IHost host = Host.CreateDefaultBuilder(args)
                       .ConfigureApplication()
                       .Build();

using IServiceScope scope = host.Services.CreateScope();

IConsoleHelper consoleHelper = scope.ServiceProvider.GetRequiredService<IConsoleHelper>();

if (args.Length > 0)
{
    if (args[0] == "--help")
    {
        consoleHelper.WriteHelpText();

        return;
    }
}

try
{
    IProcess process = scope.ServiceProvider.GetRequiredService<IProcess>();

    process.Run();
}
catch (Exception e)
{
    consoleHelper.WriteError($"{e.Message}");
}