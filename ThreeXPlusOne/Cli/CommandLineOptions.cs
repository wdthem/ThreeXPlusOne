using CommandLine;

namespace ThreeXPlusOne.Cli;

public class CommandLineOptions()
{
    [Option('h', "help", Required = false, HelpText = "Display help information.")]
    public bool Help { get; set; }

    [Option('v', "version", Required = false, HelpText = "Display version information.")]
    public bool Version { get; set; }
}