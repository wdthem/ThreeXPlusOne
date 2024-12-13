using ThreeXPlusOne.App.Helpers;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class AppPresenter(IConsoleService consoleService,
                          IUiComponent uiComponent) : IAppPresenter
{
    /// <summary>
    /// Displays the app header.
    /// </summary>
    public void DisplayAppHeader()
    {
        Clear();
        WriteAsciiArtLogo();
    }

    /// <summary>
    /// Displays the command parsing messages.
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    public void DisplayCommandParsingMessages(List<string> commandParsingMessages)
    {
        if (commandParsingMessages.Count == 0)
        {
            return;
        }

        uiComponent.WriteHeading("Information");

        foreach (string message in commandParsingMessages)
        {
            consoleService.WriteLine($"  {message}");
        }

        consoleService.WriteLine("");
    }

    /// <summary>
    /// Displays the process end message.
    /// </summary>
    /// <param name="timespan"></param>
    public void DisplayProcessEnd(TimeSpan timespan)
    {
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           timespan.Minutes, timespan.Seconds, timespan.Milliseconds);

        uiComponent.WriteHeading($"Process completed");
        consoleService.WriteLineWithColorMarkup($"  [WhiteSmoke]Execution time[/]: {elapsedTime}\n\n");
    }

    /// <summary>
    /// Clear the console screen and set the background color to VsCodeGray.
    /// </summary>
    private void Clear()
    {
        consoleService.Write(ColorHelper.DefaultAnsiBackgroundColour);

        // Clear the entire screen with the new background color
        consoleService.Write("\x1b[2J\x1b[H");

        consoleService.Write(ColorHelper.DefaultAnsiForegroundColour);
    }

    /// <summary>
    /// Write the app's ASCII art logo to the console.
    /// </summary>
    private void WriteAsciiArtLogo()
    {
        consoleService.Write("\n\n");

        consoleService.WriteLineWithColorMarkup("[SoftOrchid]_____[/][IcyBlue]/\\\\\\\\\\\\\\\\\\\\[/][SoftOrchid]_______________________________________________________[/][IcyBlue]/\\\\\\[/][SoftOrchid]_[/]        ");
        consoleService.WriteLineWithColorMarkup(" [SoftOrchid]___[/][IcyBlue]/\\\\\\///////\\\\\\[/][SoftOrchid]__________________________________________________[/][IcyBlue]/\\\\\\\\\\\\\\[/][SoftOrchid]_       ");
        consoleService.WriteLineWithColorMarkup("  [SoftOrchid]__[/][IcyBlue]\\///[/][SoftOrchid]______[/][IcyBlue]/\\\\\\[/][SoftOrchid]_______________________________[/][IcyBlue]/\\\\\\[/][SoftOrchid]_______________[/][IcyBlue]\\/////\\\\\\[/][SoftOrchid]_[/]      ");
        consoleService.WriteLineWithColorMarkup("   [SoftOrchid]_________[/][IcyBlue]/\\\\\\//[/][SoftOrchid]____[/][IcyBlue]/\\\\\\[/][SoftOrchid]____[/][IcyBlue]/\\\\\\[/][SoftOrchid]_______________[/][IcyBlue]\\/\\\\\\[/][SoftOrchid]___________________[/][IcyBlue]\\/\\\\\\[/][SoftOrchid]_[/]     ");
        consoleService.WriteLineWithColorMarkup("    [SoftOrchid]________[/][IcyBlue]\\////\\\\\\[SoftOrchid]__[/][IcyBlue]\\///\\\\\\/\\\\\\/[/][SoftOrchid]_____________[/][IcyBlue]/\\\\\\\\\\\\\\\\\\\\\\[/][SoftOrchid]_______________[/][IcyBlue]\\/\\\\\\[/][SoftOrchid]_[/]    ");
        consoleService.WriteLineWithColorMarkup("     [SoftOrchid]___________[/][IcyBlue]\\//\\\\\\[/][SoftOrchid]___[/][IcyBlue]\\///\\\\\\/[/][SoftOrchid]______________[/][IcyBlue]\\/////\\\\\\///[/][SoftOrchid]________________[/][IcyBlue]\\/\\\\\\[/][SoftOrchid]_[/]   ");
        consoleService.WriteLineWithColorMarkup("      [SoftOrchid]__[/][IcyBlue]/\\\\\\[/][SoftOrchid]______[/][IcyBlue]/\\\\\\[/][SoftOrchid]_____[/][IcyBlue]/\\\\\\/\\\\\\[/][SoftOrchid]_________________[/][IcyBlue]\\/\\\\\\[/IcyBlue][SoftOrchid]___________________[/][IcyBlue]\\/\\\\\\[/][SoftOrchid]_[/]  ");
        consoleService.WriteLineWithColorMarkup("       [SoftOrchid]_[/][IcyBlue]\\///\\\\\\\\\\\\\\\\\\/[/][SoftOrchid]____[/][IcyBlue]/\\\\\\/\\///\\\\\\[/][SoftOrchid]_______________[/][IcyBlue]\\///[/][SoftOrchid]____________________[/][IcyBlue]\\/\\\\\\[/][SoftOrchid]_ [/] ");
        consoleService.WriteLineWithColorMarkup("        [SoftOrchid]___[/][IcyBlue]\\/////////[/][SoftOrchid]_____[/][IcyBlue]\\///[/][SoftOrchid]____[/][IcyBlue]\\///[/][SoftOrchid]________________________________________[/][IcyBlue]\\///[/][SoftOrchid]__ [/] ");
        consoleService.WriteLine("");
    }
}