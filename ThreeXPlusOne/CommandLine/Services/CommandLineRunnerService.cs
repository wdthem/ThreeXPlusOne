using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;
using ThreeXPlusOne.CommandLine.Models;

namespace ThreeXPlusOne.CommandLine.Services;

public class CommandLineRunnerService(ILogger<CommandLineRunnerService> logger,
                                      CommandExecutionSettingsService commandExecutionSettingsService,
                                      IAppService appService,
                                      IHostApplicationLifetime applicationLifetime,
                                      IAppPresenter appPresenter,
                                      IConfigPresenter configPresenter,
                                      IHelpPresenter helpPresenter,
                                      IUiComponent uiComponent) : BackgroundService
{
    /// <summary>
    /// Handle any supplied command line options
    /// </summary>
    /// <param name="commandExecutionSettings"></param>
    private void HandleOptions(CommandExecutionSettings commandExecutionSettings)
    {
        if (commandExecutionSettings.CommandParsingMessages.Count > 0 && !commandExecutionSettings.ContinueExecution)
        {
            uiComponent.WriteError(string.Join(", ", commandExecutionSettings.CommandParsingMessages));
            helpPresenter.WriteCommandUsage(commandExecutionSettings.OptionsMetadata);

            return;
        }

        if (commandExecutionSettings.WriteHelpText)
        {
            appPresenter.DisplayAppHeader();
            helpPresenter.WriteHelpText(commandExecutionSettings.OptionsMetadata);

            return;
        }

        if (commandExecutionSettings.WriteVersionText)
        {
            helpPresenter.WriteVersionText();

            return;
        }

        if (commandExecutionSettings.WriteConfigText)
        {
            appPresenter.DisplayAppHeader();
            configPresenter.WriteConfigText();

            return;
        }

        //user provided a path, but the path was invalid
        if (commandExecutionSettings.AppSettingsPathProvided && !commandExecutionSettings.AppSettingsPathExists)
        {
            commandExecutionSettings.CommandParsingMessages.Add($"App settings file not found at provided path.");

            //app settings were then found at the default location
            if (!string.IsNullOrEmpty(commandExecutionSettings.AppSettingsFileFullPath))
            {
                commandExecutionSettings.CommandParsingMessages.Add("App settings file found at default location (current execution directory).");
            }
        }

        //user provided a path, and it was valid
        if (commandExecutionSettings.AppSettingsPathProvided && commandExecutionSettings.AppSettingsPathExists)
        {
            commandExecutionSettings.CommandParsingMessages.Add($"App settings file found at provided path: {commandExecutionSettings.AppSettingsFileFullPath}");
        }

        //no app settings were found at either the provided or default locations
        if (string.IsNullOrEmpty(commandExecutionSettings.AppSettingsFileFullPath))
        {
            commandExecutionSettings.CommandParsingMessages.Add("App settings file not found at default location. Defaults used instead.");
        }
    }

    /// <summary>
    /// Run the app based on the settings parsed by the CommandLineParser
    /// </summary>
    /// <param name="commandExecutionSettings"></param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            HandleOptions(commandExecutionSettingsService.Settings);

            if (commandExecutionSettingsService.Settings.ContinueExecution)
            {
                //run the app
                await appService.Run(commandExecutionSettingsService.Settings.CommandParsingMessages);
            }

            applicationLifetime.StopApplication();
        }
        catch (Exception ex)
        {
            uiComponent.WriteError(ex.Message);
            logger.LogError("{error}", ex.Message.Replace("\r", "").Replace("\n", " "));
            applicationLifetime.StopApplication();
        }
    }
}