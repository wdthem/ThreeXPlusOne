using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.CommandLine.Models;

namespace ThreeXPlusOne.CommandLine;

public class CommandLineRunner(IProcess process,
                               IConsoleService consoleService)
{
    /// <summary>
    /// Run the app
    /// </summary>
    private void RunApp(List<string> commandParsingMessages)
    {
        try
        {
            process.Run(commandParsingMessages);
        }
        catch (Exception e)
        {
            consoleService.WriteError(e.Message);
        }
    }

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
        }

        if (commandExecutionSettings.WriteHelpText)
        {
            consoleService.WriteHelpText(commandExecutionSettings.OptionsMetadata);
        }

        if (commandExecutionSettings.WriteVersionText)
        {
            consoleService.WriteVersionText();
        }

        if (commandExecutionSettings.WriteConfigText)
        {
            consoleService.WriteConfigText();
        }

        //user provided a path, but the path was invalid
        if (commandExecutionSettings.SettingsPathProvided && !commandExecutionSettings.SettingsPathExists)
        {
            commandExecutionSettings.CommandParsingMessages.Add($"Settings file not found at provided path.");

            //settings were then found at the default location
            if (!string.IsNullOrEmpty(commandExecutionSettings.SettingsFileFullPath))
            {
                commandExecutionSettings.CommandParsingMessages.Add("Settings file found at default location (current execution directory).");
            }
        }

        //user provided a path, and it was valid
        if (commandExecutionSettings.SettingsPathProvided && commandExecutionSettings.SettingsPathExists)
        {
            commandExecutionSettings.CommandParsingMessages.Add($"Settings file found at provided path: {commandExecutionSettings.SettingsFileFullPath}");
        }

        //no settings were found at either the provided or default locations
        if (string.IsNullOrEmpty(commandExecutionSettings.SettingsFileFullPath))
        {
            commandExecutionSettings.CommandParsingMessages.Add("Settings file not found at default location. Defaults used instead.");
        }
    }

    /// <summary>
    /// Run the command based on the settings parsed by the CommandLineParser
    /// </summary>
    /// <param name="commandExecutionSettings"></param>
    public void RunCommand(CommandExecutionSettings commandExecutionSettings)
    {
        HandleOptions(commandExecutionSettings);

        if (commandExecutionSettings.ContinueExecution)
        {
            RunApp(commandExecutionSettings.CommandParsingMessages);
        }
    }
}