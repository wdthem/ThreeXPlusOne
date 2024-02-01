using System.Reflection;
using CommandLine;
using Microsoft.Extensions.Hosting;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.Helpers;
using ThreeXPlusOne.Cli.Models;

namespace ThreeXPlusOne.Cli;

public class CommandLineInterface(IProcess process,
                                  IConsoleHelper consoleHelper)
{
    private readonly IProcess _process = process;
    private readonly IConsoleHelper _consoleHelper = consoleHelper;

    /// <summary>
    /// Run the app
    /// </summary>
    private void RunApp()
    {
        try
        {
            _process.Run();
        }
        catch (Exception e)
        {
            _consoleHelper.WriteError(e.Message);
        }
    }

    /// <summary>
    /// Handle command-line options and determine if execution flow should continue
    /// </summary>
    /// <param name="opts"></param>
    /// <returns></returns>
    private CommandExecutionSettings HandleOptions(CommandLineOptions opts)
    {
        CommandExecutionSettings flow = new() { ContinueExecution = true };

        if (opts.Help)
        {
            List<(string longName, string shortName, string description)> options = GetOptionsAttributeMetadata();

            _consoleHelper.WriteHelpText(options);

            flow.ContinueExecution = false;
        }

        if (opts.Version)
        {
            _consoleHelper.WriteVersionText();

            flow.ContinueExecution = false;
        }

        if (opts.Usage)
        {
            _consoleHelper.WriteUsageText();

            flow.ContinueExecution = false;
        }

        if (!string.IsNullOrWhiteSpace(opts.SettingsPath))
        {
            string directoryPath = GetDirectoryPath(opts.SettingsPath);
            flow.SettingsPath = Path.Combine(directoryPath, "settings.json");

            if (!File.Exists(flow.SettingsPath))
            {
                _consoleHelper.WriteError($"File 'settings.json' not found at provided path: {directoryPath}. Using defaults.");

                flow.SettingsPath = "";
            }
        }

        return flow;
    }

    /// <summary>
    /// Generate a list of all option values to send to the console
    /// </summary>
    /// <returns></returns>
    private static List<(string longName, string shortName, string description)> GetOptionsAttributeMetadata()
    {
        List<(string longName, string shortName, string description)> options = [];

        Type optionsType = typeof(CommandLineOptions);

        foreach (PropertyInfo prop in optionsType.GetProperties())
        {
            OptionAttribute? optionAttribute = prop.GetCustomAttribute<OptionAttribute>();

            if (optionAttribute != null)
            {
                options.Add((optionAttribute.LongName, optionAttribute.ShortName, optionAttribute.HelpText));
            }
        }

        return options;
    }

    /// <summary>
    /// The --settings argument expects only a directory, as the settings file is set to a reserved name
    /// Get the directory path handling multiple scenarios
    /// </summary>
    /// <param name="userInput"></param>
    /// <returns></returns>
    private static string GetDirectoryPath(string userInput)
    {
        if (File.Exists(userInput))
        {
            return Path.GetDirectoryName(userInput) ?? "";
        }
        else if (Directory.Exists(userInput))
        {
            return userInput;
        }
        else
        {
            return Path.GetDirectoryName(userInput) ?? userInput;
        }
    }

    /// <summary>
    /// Parse the command and any provided arguments
    /// </summary>
    /// <param name="host"></param>
    /// <param name="arguments"></param>
    public void RunCommand(IHost host, string[] arguments)
    {
        CommandExecutionSettings? flow = null;

        Parser parser = new(settings =>
        {
            settings.AutoHelp = false;
            settings.AutoVersion = false;
        });

        parser.ParseArguments<CommandLineOptions>(arguments)
              .WithParsed(options =>
                {
                    flow = HandleOptions(options);
                })
              .WithNotParsed(errors => { });

        if (flow == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(flow.SettingsPath))
        {

        }

        if (flow.ContinueExecution)
        {
            RunApp();
        }
    }
}