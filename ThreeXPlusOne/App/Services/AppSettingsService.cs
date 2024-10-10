using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public class AppSettingsService(IOptions<AppSettings> appSettings,
                                IFileService fileService,
                                IConsoleService consoleService) : IAppSettingsService
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Allow the user to save the generated number list to app settings for future use.
    /// </summary>
    public async Task SaveGeneratedNumbers()
    {
        consoleService.WriteHeading("Save app settings");

        bool confirmedSaveSettings = _appSettings.AlgorithmSettings.FromRandomNumbers &&
                                     consoleService.ReadYKeyToProceed($"Save generated number series to '{_appSettings.SettingsFileFullPath}' for reuse?");

        await fileService.WriteSettingsToFile(confirmedSaveSettings);

        consoleService.WriteSettingsSavedMessage(confirmedSaveSettings);
    }
}