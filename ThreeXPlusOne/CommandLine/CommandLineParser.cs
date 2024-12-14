using System.Reflection;
using CommandLine;
using ThreeXPlusOne.CommandLine.Models;

namespace ThreeXPlusOne.CommandLine;

public static class CommandLineParser
{
    /// <summary>
    /// Handle command-line options and determine if execution flow should continue
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    private static CommandExecutionSettings ParseOptions(CommandLineOptions options)
    {
        CommandExecutionSettings commandExecutionSettings = new() { ContinueExecution = true };

        if (options.Help)
        {
            commandExecutionSettings.OptionsMetadata = GetOptionsAttributeMetadata();
            commandExecutionSettings.WriteHelpText = true;
            commandExecutionSettings.ContinueExecution = false;
        }

        if (options.Version)
        {
            commandExecutionSettings.WriteVersionText = true;
            commandExecutionSettings.ContinueExecution = false;
        }

        if (options.Config)
        {
            commandExecutionSettings.WriteConfigText = true;
            commandExecutionSettings.ContinueExecution = false;
        }

        //look for app settings file at provided directory
        if (!string.IsNullOrWhiteSpace(options.SettingsPath))
        {
            commandExecutionSettings.AppSettingsFileFullPath = "";
            commandExecutionSettings.AppSettingsPathProvided = true;
            commandExecutionSettings.AppSettingsPathExists = false;

            string trimmedPath = options.SettingsPath.Trim();

            string directoryPath = GetDirectoryPath(trimmedPath);

            string combinedPath = Path.Combine(directoryPath, commandExecutionSettings.AppSettingsFileName);

            if (File.Exists(combinedPath))
            {
                commandExecutionSettings.AppSettingsPathExists = true;
                commandExecutionSettings.AppSettingsFileFullPath = combinedPath;
            }
        }

        //else look for app settings file at current execution directory
        if (string.IsNullOrWhiteSpace(commandExecutionSettings.AppSettingsFileFullPath))
        {
            if (File.Exists(commandExecutionSettings.AppSettingsFileName))
            {
                commandExecutionSettings.AppSettingsFileFullPath = commandExecutionSettings.AppSettingsFileName;
            }

            //otherwise defaults are used
        }

        return commandExecutionSettings;
    }

    /// <summary>
    /// Generate a list of all possible option values to send to the console as help text
    /// </summary>
    /// <returns></returns>
    private static List<(string shortName, string longName, string description, string hint)> GetOptionsAttributeMetadata()
    {
        List<(string shortName, string longName, string description, string hint)> options = [];

        Type optionsType = typeof(CommandLineOptions);

        foreach (PropertyInfo propertyInfo in optionsType.GetProperties())
        {
            OptionAttribute? optionAttribute = propertyInfo.GetCustomAttribute<OptionAttribute>();
            CommandLineHintAttribute? commandLineOptionAttribute = propertyInfo.GetCustomAttribute<CommandLineHintAttribute>();

            string hint = commandLineOptionAttribute != null ? commandLineOptionAttribute.Hint : "";

            if (optionAttribute != null)
            {
                options.Add((optionAttribute.ShortName, optionAttribute.LongName, optionAttribute.HelpText, hint));
            }
        }

        return options;
    }

    /// <summary>
    /// The --settings argument expects only a directory, as the app settings file is set to a reserved name
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
    /// <param name="arguments"></param>
    public static CommandExecutionSettings ParseCommand(string[] arguments)
    {
        CommandExecutionSettings commandExecutionSettings = new();

        Parser parser = new(settings =>
        {
            settings.AutoHelp = false;
            settings.AutoVersion = false;
        });

        parser.ParseArguments<CommandLineOptions>(arguments)
              .WithParsed(options =>
                {
                    commandExecutionSettings = ParseOptions(options);
                })
              .WithNotParsed(errors =>
              {
                  foreach (Error? error in errors)
                  {
                      string? errorText = null;

                      if (error is UnknownOptionError)
                      {
                          string tokenText = error is UnknownOptionError optionError ? $" (-{optionError.Token})" : "";

                          errorText = $"Unknown command option{tokenText}";

                          commandExecutionSettings.OptionsMetadata = GetOptionsAttributeMetadata();
                          commandExecutionSettings.ContinueExecution = false;
                      }
                      else
                      {
                          errorText = error.ToString();
                      }

                      if (errorText == null)
                      {
                          continue;
                      }

                      commandExecutionSettings.CommandParsingMessages.Add(errorText);
                  }
              });

        return commandExecutionSettings;
    }
}