using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class AppSettingsPresenter(IOptions<AppSettings> appSettings,
                                  IConsoleService consoleService,
                                  IUiComponent uiComponent) : IAppSettingsPresenter
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Output a message regarding whether the user saved the app settings file or not.
    /// </summary>
    /// <param name="savedSettings"></param>
    public void WriteSettingsSavedMessage(bool savedSettings)
    {
        if (savedSettings)
        {
            consoleService.WriteLine($"  Saved generated numbers to '{_appSettings.SettingsFileFullPath}'");
        }
        else
        {
            consoleService.WriteLine("  Settings left unchanged");
        }
    }

    /// <summary>
    /// Display a header with the given text.
    /// </summary>
    /// <param name="header"></param>
    public void DisplayHeader(string header)
    {
        uiComponent.WriteHeading(header);
    }

    /// <summary>
    /// Get a confirmation from the user.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool GetConfirmation(string message)
    {
        uiComponent.WriteSeparator();

        bool result = uiComponent.AskForConfirmation(message);

        uiComponent.WriteSeparator();

        return result;
    }
}