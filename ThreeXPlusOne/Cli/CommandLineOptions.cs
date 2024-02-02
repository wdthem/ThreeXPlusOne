using CommandLine;

namespace ThreeXPlusOne.Cli;

public class CommandLineOptions()
{
    [Option('h', "help", Required = false, HelpText = "Display help information.")]
    public bool Help { get; set; }

    [Option('v', "version", Required = false, HelpText = "Display version information.")]
    public bool Version { get; set; }

    [Option('u', "usage", Required = false, HelpText = "Display usage information.")]
    public bool Usage { get; set; }

    [Option('s', "settings", Required = false, HelpText = "Specifies the directory path to the settings file.")]
    public string? SettingsPath { get; set; }
}