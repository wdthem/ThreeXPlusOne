using Microsoft.Extensions.Options;
using System.Diagnostics;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App;

public class Process(IOptions<AppSettings> appSettings,
                     IDirectedGraphService directedGraphService,
                     IFileService fileService,
                     IConsoleService consoleService) : IScopedService
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Run the algorithm and data generation based on the user-provided app settings.
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    public async Task Run(List<string> commandParsingMessages)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        consoleService.WriteAsciiArtLogo();
        consoleService.WriteCommandParsingMessages(commandParsingMessages);
        consoleService.WriteSettings();

        await directedGraphService.GenerateDirectedGraph(stopwatch);

        await SaveSettings();

        stopwatch.Stop();

        consoleService.WriteProcessEnd(stopwatch.Elapsed);
    }

    /// <summary>
    /// Allow the user to save the generated number list to app settings for future use.
    /// </summary>
    private async Task SaveSettings()
    {
        consoleService.WriteHeading("Save app settings");

        bool confirmedSaveSettings = _appSettings.AlgorithmSettings.FromRandomNumbers &&
                                     consoleService.ReadYKeyToProceed($"Save generated number series to '{_appSettings.SettingsFileFullPath}' for reuse?");

        await fileService.WriteSettingsToFile(confirmedSaveSettings);
        consoleService.WriteSettingsSavedMessage(confirmedSaveSettings);
    }
}