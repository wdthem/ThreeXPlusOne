using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Code.Interfaces;

if (args.Length > 0)
{
    if (args[0] == "--help")
    {
        ConsoleOutput.WriteHelpText();

        return;
    }
}

using IHost host = Host.CreateDefaultBuilder(args)
                       .ConfigureApplication()
                       .Build();

try
{
    using IServiceScope scope = host.Services.CreateScope();

    IProcess process = scope.ServiceProvider.GetRequiredService<IProcess>();

    process.Run();
}
catch (Exception e)
{
    ConsoleOutput.WriteError($"{e.Message}");
}