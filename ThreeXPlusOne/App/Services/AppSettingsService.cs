using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Services;

public class AppSettingsService(IOptions<AppSettings> appSettings,
                                IFileService fileService,
                                IAppSettingsPresenter appSettingsPresenter) : IAppSettingsService
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Allow the user to save the generated number list to app settings for future use.
    /// </summary>
    public async Task UpdateAppSettingsFile()
    {
        appSettingsPresenter.DisplayHeader("Save app settings");

        bool confirmedSaveSettings = _appSettings.AlgorithmSettings.FromRandomNumbers &&
                                     appSettingsPresenter.GetConfirmation($"Save generated number series to '{_appSettings.SettingsFileFullPath}' for reuse?");

        await fileService.WriteSettingsToFile(confirmedSaveSettings);

        appSettingsPresenter.WriteSettingsSavedMessage(confirmedSaveSettings);
    }
}