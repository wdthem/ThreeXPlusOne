using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Enums.Extensions;
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
        consoleService.WriteLineWithMarkup($"  [WhiteSmoke]Execution time[/]: {elapsedTime}\n\n");
    }

    /// <summary>
    /// Clear the console screen and set the background color to VsCodeGray.
    /// </summary>
    private void Clear()
    {
        consoleService.Write(BaseColor.Background.ToAnsiCode());
        consoleService.Write(AnsiCode.ClearScreen.GetCode());
        consoleService.Write(BaseColor.Foreground.ToAnsiCode());
    }

    /// <summary>
    /// Write the app's ASCII art logo to the console.
    /// </summary>
    private void WriteAsciiArtLogo()
    {
        consoleService.Write("\n\n");

        consoleService.WriteLineWithMarkup("[SoftOrchid]_____[/][IcyBlue]/\\\\\\\\\\\\\\\\\\\\ [/][SoftOrchid]_______________________________________________________[/][IcyBlue]/\\\\\\ [/][SoftOrchid]_[/]        ");
        consoleService.WriteLineWithMarkup(" [SoftOrchid]___[/][IcyBlue]/\\\\\\///////\\\\\\ [/][SoftOrchid]__________________________________________________[/][IcyBlue]/\\\\\\\\\\\\\\ [/][SoftOrchid]_       ");
        consoleService.WriteLineWithMarkup("  [SoftOrchid]__[/][IcyBlue]\\///[/][SoftOrchid]______[/][IcyBlue]/\\\\\\ [/][SoftOrchid]______________________________[/][IcyBlue]/\\\\\\ [/][SoftOrchid]_______________[/][IcyBlue]\\/////\\\\\\ [/][SoftOrchid]_[/]      ");
        consoleService.WriteLineWithMarkup("   [SoftOrchid]_________[/][IcyBlue]/\\\\\\//[/][SoftOrchid]____[/][IcyBlue]/\\\\\\ [/][SoftOrchid]___[/][IcyBlue]/\\\\\\ [/][SoftOrchid]______________[/][IcyBlue]\\/\\\\\\ [/][SoftOrchid]___________________[/][IcyBlue]\\/\\\\\\ [/][SoftOrchid]_[/]     ");
        consoleService.WriteLineWithMarkup("    [SoftOrchid]________[/][IcyBlue]\\////\\\\\\ [SoftOrchid]_[/][IcyBlue]\\///\\\\\\/\\\\\\/[/][SoftOrchid]_____________[/][IcyBlue]/\\\\\\\\\\\\\\\\\\\\\\ [/][SoftOrchid]_______________[/][IcyBlue]\\/\\\\\\ [/][SoftOrchid]_[/]    ");
        consoleService.WriteLineWithMarkup("     [SoftOrchid]___________[/][IcyBlue]\\//\\\\\\ [/][SoftOrchid]__[/][IcyBlue]\\///\\\\\\/[/][SoftOrchid]______________[/][IcyBlue]\\/////\\\\\\///[/][SoftOrchid]_________________[/][IcyBlue]\\/\\\\\\ [/][SoftOrchid]_[/]   ");
        consoleService.WriteLineWithMarkup("      [SoftOrchid]__[/][IcyBlue]/\\\\\\ [/][SoftOrchid]_____[/][IcyBlue]/\\\\\\ [/][SoftOrchid]____[/][IcyBlue]/\\\\\\/\\\\\\ [/][SoftOrchid]________________[/][IcyBlue]\\/\\\\\\ [/IcyBlue][SoftOrchid]___________________[/][IcyBlue]\\/\\\\\\ [/][SoftOrchid]_[/]  ");
        consoleService.WriteLineWithMarkup("       [SoftOrchid]_[/][IcyBlue]\\///\\\\\\\\\\\\\\\\\\/[/][SoftOrchid]____[/][IcyBlue]/\\\\\\/\\///\\\\\\ [/][SoftOrchid]______________[/][IcyBlue]\\///[/][SoftOrchid]_____________________[/][IcyBlue]\\/\\\\\\ [/][SoftOrchid]_ [/] ");
        consoleService.WriteLineWithMarkup("        [SoftOrchid]___[/][IcyBlue]\\/////////[/][SoftOrchid]_____[/][IcyBlue]\\///[/][SoftOrchid]____[/][IcyBlue]\\///[/][SoftOrchid]_________________________________________[/][IcyBlue]\\///[/][SoftOrchid]___ [/] ");
        consoleService.WriteLine("");
    }
}