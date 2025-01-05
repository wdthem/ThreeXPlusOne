using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Enums.Extensions;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters.Components;

public class UiComponent(IConsoleService consoleService) : IUiComponent
{
    private readonly string _consoleLine = new('═', consoleService.AppConsoleWidth);

    /// <summary>
    /// Write an error message.
    /// </summary>
    /// <param name="message"></param>
    public void WriteError(string message)
    {
        consoleService.WriteWithMarkup($"  {Emoji.RedX.GetUnicodeValue()} {message}\n");
    }

    /// <summary>
    /// Write a message indicating the given step completed.
    /// </summary>
    public void WriteDone()
    {
        consoleService.WriteLineWithMarkup("[BrightJade]Done[/]");
    }

    /// <summary>
    /// Read the y or n key press by the user to know whether or not to proceed.
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    public bool AskForConfirmation(string prompt)
    {
        consoleService.WriteWithMarkup($"  {Emoji.ThinkingFace.GetUnicodeValue()} [EmojiYellow]{prompt}[/] [WhiteSmoke]\\[y/n\\][/] ");

        while (true)
        {
            ConsoleKey key = Console.ReadKey(intercept: true).Key;
            string response = key == ConsoleKey.Y ? "y" : "n";

            consoleService.WriteLine(response);

            return key == ConsoleKey.Y;
        }
    }

    /// <summary>
    /// Write a visual separator in console output.
    /// </summary>
    public void WriteSeparator()
    {
        consoleService.WriteLineWithMarkup($"\n  [MediumGray]{_consoleLine[..^4]}[/]\n");
    }

    /// <summary>
    /// Write a section heading in console output.
    /// </summary>
    /// <param name="headerText"></param>
    public void WriteHeading(string headerText)
    {
        consoleService.WriteLineWithMarkup($"\n[SoftOrchid]╔{_consoleLine[..^2]}╗[/]");
        consoleService.WriteWithMarkup("[SoftOrchid]║[/]");
        consoleService.WriteWithMarkup($" [IcyBlue]{headerText.ToUpper().PadRight(_consoleLine.Length - 3)}[/]");
        consoleService.WriteLineWithMarkup("[SoftOrchid]║[/]");
        consoleService.WriteLineWithMarkup($"[SoftOrchid]╚{_consoleLine[..^2]}╝[/]");
    }
}