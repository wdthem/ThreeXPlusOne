using CommandLine;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.Helpers;

namespace ThreeXPlusOne.Cli;

public class CommandLineInterface(IProcess process,
                                  IConsoleHelper consoleHelper)
{
    private readonly IProcess _process = process;
    private readonly IConsoleHelper _consoleHelper = consoleHelper;

    /// <summary>
    /// Run the ThreeXPlusOne app
    /// </summary>
    private void RunThreeXPlusOne()
    {
        try
        {
            _process.Run();
        }
        catch (Exception e)
        {
            _consoleHelper.WriteError($"{e.Message}");
        }
    }

    /// <summary>
    /// Parse the command and any provided arguments
    /// </summary>
    /// <param name="args"></param>
    public void RunCommandWithArguments(string[] args)
    {
        Parser parser = new(settings =>
        {
            settings.AutoHelp = false;
            settings.AutoVersion = false;
        });

        parser.ParseArguments<CommandLineOptions>(args)
                .WithParsed(opts =>
                {
                    if (opts.Help)
                    {
                        _consoleHelper.WriteHelpText();

                        return;
                    }

                    if (opts.Version)
                    {
                        _consoleHelper.WriteVersionText();

                        return;
                    }

                    RunThreeXPlusOne();

                })
                .WithNotParsed(errs =>
                {

                });
    }
}