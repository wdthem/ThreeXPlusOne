using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.Helpers;
using ThreeXPlusOne.Cli.Models;

namespace ThreeXPlusOne.Cli;

public class CommandLineRunner(IProcess process,
                               IConsoleHelper consoleHelper)
{
    private readonly IProcess _process = process;
    private readonly IConsoleHelper _consoleHelper = consoleHelper;

    /// <summary>
    /// Run the app
    /// </summary>
    private void RunApp(List<string> commandParsingMessages)
    {
        try
        {
            _process.Run(commandParsingMessages);
        }
        catch (Exception e)
        {
            _consoleHelper.WriteError(e.Message);
        }
    }

    /// <summary>
    /// Run the command based on the settings parsed by the CommandLineParser
    /// </summary>
    /// <param name="commandExecutionSettings"></param>
    public void RunCommand(CommandExecutionSettings commandExecutionSettings)
    {
        List<string> commandParsingMessages = [];

        if (commandExecutionSettings.WriteHelpText)
        {
            _consoleHelper.WriteHelpText(commandExecutionSettings.OptionsMetadata);
        }

        if (commandExecutionSettings.WriteVersionText)
        {
            _consoleHelper.WriteVersionText();
        }

        if (commandExecutionSettings.WriteUsageText)
        {
            _consoleHelper.WriteUsageText();
        }

        if (commandExecutionSettings.SettingsPathProvided && !commandExecutionSettings.SettingsPathExists)
        {
            commandParsingMessages.Add($"Settings file not found at provided path.");

            if (!string.IsNullOrEmpty(commandExecutionSettings.SettingsPath))
            {
                commandParsingMessages.Add("Settings file found at default location (current execution directory).");
            }
        }

        if (string.IsNullOrEmpty(commandExecutionSettings.SettingsPath))
        {
            commandParsingMessages.Add("Settings file not found at default location. Defaults used instead.");
        }

        if (commandExecutionSettings.ContinueExecution)
        {
            RunApp(commandParsingMessages);
        }
    }
}