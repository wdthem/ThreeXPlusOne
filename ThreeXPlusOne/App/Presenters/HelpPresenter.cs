using System.Reflection;
using ThreeXPlusOne.App.Helpers;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class HelpPresenter(IConsoleService consoleService,
                           IUiComponent uiComponent) : IHelpPresenter
{
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// Write the app's help text to the console.
    /// </summary>
    /// <param name="commandLineOptions"></param>
    public void WriteHelpText(List<(string longName, string shortName, string description, string hint)> commandLineOptions)
    {
        uiComponent.WriteHeading("Help");

        uiComponent.WriteHeading("Commands");

        WriteCommandUsage(commandLineOptions);

        uiComponent.WriteHeading("Version");
        WriteVersionText();

        uiComponent.WriteHeading("GitHub repository");
        consoleService.WriteLine($"  {HrefHelper.GetAnsiHyperlink("https://github.com/wdthem/ThreeXPlusOne", "https://github.com/wdthem/ThreeXPlusOne")}\n");

        uiComponent.WriteHeading("Credits");
        consoleService.WriteLine($"  Inspiration from Veritasium: {HrefHelper.GetAnsiHyperlink("https://www.youtube.com/watch?v=094y1Z2wpJg", "https://www.youtube.com/watch?v=094y1Z2wpJg")}");
        consoleService.WriteLine($"  ASCII art via:               {HrefHelper.GetAnsiHyperlink("https://www.patorjk.com/software/taag/#p=display", "https://www.patorjk.com/software/taag/#p=display")}");
        consoleService.WriteLine($"  Graphs drawn with SkiaSharp: {HrefHelper.GetAnsiHyperlink("https://github.com/mono/SkiaSharp", "https://github.com/mono/SkiaSharp")}\n\n");
    }

    /// <summary>
    /// Write the app's version information to the console.
    /// </summary>
    public void WriteVersionText()
    {
        AssemblyInformationalVersionAttribute? versionAttribute =
            _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        if (versionAttribute != null)
        {
            string version = versionAttribute.InformationalVersion;
            string[] versionParts = version.Split('+');
            string coreVersion = versionParts[0];

            string? assemblyName = _assembly.GetName().Name;

            consoleService.WriteLineWithColorMarkup($"  [BrightJade]{assemblyName}[/]: v{coreVersion}\n");
        }
        else
        {
            consoleService.WriteLineWithColorMarkup("  Version information not found.\n");
        }
    }

    /// <summary>
    /// Write the app's usage info to the console.
    /// </summary>
    /// <param name="commandLineOptions"></param>
    public void WriteCommandUsage(List<(string longName, string shortName, string description, string hint)> commandLineOptions)
    {
        string? assemblyName = _assembly.GetName().Name;

        consoleService.WriteWithColorMarkup("\n  usage: ");
        consoleService.WriteWithColorMarkup($"[BrightJade]{assemblyName}[/] ");

        int lcv = 1;
        foreach ((string shortName, string longName, string description, string hint) in commandLineOptions)
        {
            if (lcv % 3 == 0)
            {
                consoleService.Write("\n                       ");
            }

            string hintText = !string.IsNullOrWhiteSpace(hint) ? $" {hint}" : "";

            consoleService.WriteWithColorMarkup($"[[IcyBlue]-{shortName}[/] | [IcyBlue]--{longName}[/]] ");

            lcv++;
        }

        consoleService.WriteLine("\n");

        foreach ((string shortName, string longName, string description, string hint) in commandLineOptions)
        {
            string commandText = $"  -{shortName}, --{longName}";


            consoleService.WriteWithColorMarkup($"  [IcyBlue]-{shortName}[/], [IcyBlue]--{longName}[/]");

            if (commandText.Length <= 15)
            {
                consoleService.Write("\t\t");
            }
            else
            {
                consoleService.Write("\t");
            }

            consoleService.WriteLineWithColorMarkup($"{description}");
        }

        consoleService.WriteLine("");
    }
}