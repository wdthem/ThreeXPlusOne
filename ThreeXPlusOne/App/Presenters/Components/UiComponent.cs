using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;

namespace ThreeXPlusOne.App.Presenters.Components;

public class UiComponent(IConsoleService consoleService) : IUiComponent
{
    /// <summary>
    /// Write an error message.
    /// </summary>
    /// <param name="message"></param>
    public void WriteError(string message)
    {
        consoleService.WriteWithColorMarkup($"\n[BlushRed]Error:[/] {message}\n");
    }

    /// <summary>
    /// Write a message indicating the given step completed.
    /// </summary>
    public void WriteDone()
    {
        consoleService.WriteLineWithColorMarkup("[BrightJade]Done[/]");
    }

    /// <summary>
    /// Read the y or n key press by the user to know whether or not to proceed.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool AskForConfirmation(string message)
    {
        consoleService.WriteWithColorMarkup($"[PureYellow]{message}[/] [[WhiteSmoke]y/n[/]] ");

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
    public void WriteSeparator(bool delay = false)
    {
        consoleService.WriteWithColorMarkup("\n[MediumGray]————————————————————————————————————————————————————————————————————————————————————[/]\n");
    }

    /// <summary>
    /// Write a section heading in console output.
    /// </summary>
    /// <param name="headerText"></param>
    public void WriteHeading(string headerText)
    {
        WriteSeparator();

        consoleService.WriteWithColorMarkup($"[BlueTint]{headerText.ToUpper()}[/]");

        WriteSeparator();
    }
}