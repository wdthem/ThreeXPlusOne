using System.Diagnostics;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App;

public class Process(IDirectedGraphService directedGraphService,
                     IAppSettingsService appSettingsService,
                     IConsoleService consoleService) : IScopedService
{
    /// <summary>
    /// Generated the directed graph based on the user-provided app settings.
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    public async Task Run(List<string> commandParsingMessages)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        consoleService.WriteAsciiArtLogo();
        consoleService.WriteCommandParsingMessages(commandParsingMessages);
        consoleService.WriteSettings();

        await directedGraphService.GenerateDirectedGraph();

        await appSettingsService.SaveGeneratedNumbers();

        stopwatch.Stop();

        consoleService.WriteProcessEnd(stopwatch.Elapsed);
    }
}