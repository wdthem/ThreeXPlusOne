using ThreeXPlusOne.App;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.CommandLine.Models;

namespace ThreeXPlusOne.CommandLine;

public class CommandLineRunner(Process process,
                               IConsoleService consoleService)
{
    /// <summary>
    /// Handle any supplied command line options
    /// </summary>
    /// <param name="commandExecutionSettings"></param>
    private void HandleOptions(CommandExecutionSettings commandExecutionSettings)
    {
        if (commandExecutionSettings.CommandParsingMessages.Count > 0 && !commandExecutionSettings.ContinueExecution)
        {
            consoleService.WriteError(string.Join(", ", commandExecutionSettings.CommandParsingMessages));
            consoleService.WriteCommandUsage(commandExecutionSettings.OptionsMetadata);

            return;
        }

        if (commandExecutionSettings.WriteHelpText)
        {
            consoleService.WriteHelpText(commandExecutionSettings.OptionsMetadata);

            return;
        }

        if (commandExecutionSettings.WriteVersionText)
        {
            consoleService.WriteVersionText();

            return;
        }

        if (commandExecutionSettings.WriteConfigText)
        {
            consoleService.WriteConfigText();

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
    /// Run the command based on the settings parsed by the CommandLineParser
    /// </summary>
    /// <param name="commandExecutionSettings"></param>
    public void RunCommand(CommandExecutionSettings commandExecutionSettings)
    {
        try
        {
            HandleOptions(commandExecutionSettings);

            if (commandExecutionSettings.ContinueExecution)
            {
                //run the app
                process.Run(commandExecutionSettings.CommandParsingMessages);
            }
        }
        catch (Exception e)
        {
            consoleService.WriteError(e.Message);
        }
    }
}