using System.Reflection;
using CommandLine;
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
    /// Handle optional parameters and determine if execution flow should continue
    /// </summary>
    /// <param name="opts"></param>
    /// <returns></returns>
    private CommandExecutionFlow HandleOptions(CommandLineOptions opts)
    {
        CommandExecutionFlow flow = new() { Continue = true };

        if (opts.Help)
        {
            List<(string longName, string shortName, string description)> options = GetOptionsAttributeMetadata();

            _consoleHelper.WriteHelpText(options);

            flow.Continue = false;
        }

        if (opts.Version)
        {
            _consoleHelper.WriteVersionText();

            flow.Continue = false;
        }

        if (opts.Usage)
        {
            _consoleHelper.WriteUsageText();

            flow.Continue = false;
        }

        return flow;
    }

    private static List<(string longName, string shortName, string description)> GetOptionsAttributeMetadata()
    {
        List<(string longName, string shortName, string description)> options = [];

        Type optionsType = typeof(CommandLineOptions);

        foreach (PropertyInfo prop in optionsType.GetProperties())
        {
            var optionAttribute = prop.GetCustomAttribute<OptionAttribute>();

            if (optionAttribute != null)
            {
                options.Add((optionAttribute.LongName, optionAttribute.ShortName, optionAttribute.HelpText));
            }
        }

        return options;
    }

    /// <summary>
    /// Parse the command and any provided arguments
    /// </summary>
    /// <param name="arguments"></param>
    public void RunCommand(string[] arguments)
    {
        CommandExecutionFlow? flow = null;

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

        if (flow != null &&
            flow.Continue)
        {
            RunApp();
        }
    }
}