using CommandLine;

namespace ThreeXPlusOne.CommandLine.Models;

public class CommandLineOptions()
{
    [Option('h', "help", Required = false, HelpText = "Display help information.")]
    public bool Help { get; set; }

    [Option('v', "version", Required = false, HelpText = "Display version information.")]
    public bool Version { get; set; }

    [Option('c', "config", Required = false, HelpText = "Display config (settings) information.")]
    public bool Config { get; set; }

    [Option('s', "settings", Required = false, HelpText = "Specifies the directory path to the settings file.")]
    [CommandLineHint(hint: "<directory-path>")]
    public string? SettingsPath { get; set; }
}