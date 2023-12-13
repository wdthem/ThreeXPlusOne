using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;


if (args.Length > 0)
{
    if (args[0] == "--help")
    {
        ConsoleOutput.WriteHelpText();

        return;
    }
}

using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((context, builder) =>
            {
                builder.AddJsonFile("settings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddServices(context.Configuration);
            })
            .Build();

try
{
    var process = host.Services.GetRequiredService<IProcess>();

    process.Run();
}
catch(Exception e)
{
    ConsoleOutput.WriteError($"{e.Message}");
}