using ThreeXPlusOne.App.Services.Base;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Services;

public class ConsoleService(IMarkupService markupService) : ConsoleServiceBase, IConsoleService
{
    private static readonly Lock _consoleLock = new();

    /// <summary>
    /// The width of the app in the console window.
    /// </summary>
    public int AppConsoleWidth => 87;

    /// <summary>
    /// Custom write method for console output (threadsafe).
    /// </summary>
    /// <param name="message"></param>
    /// <param name="delay"></param>
    public void Write(string message)
    {
        lock (_consoleLock)
        {
            Console.Write(message);

            return;
        }
    }

    /// <summary>
    /// Custom WriteLine method for console output (threadsafe).
    /// </summary>
    /// <param name="message"></param>
    public void WriteLine(string message)
    {
        lock (_consoleLock)
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Write a message to the console with markup via ANSI escape codes for full color and formatting support.
    /// </summary>
    /// <param name="message"></param>
    public void WriteWithMarkup(string message)
    {
        Write(markupService.GetDecoratedMessage(message));
    }

    /// <summary>
    /// Write an API action message to the console with color markup as a single line.
    /// </summary>
    /// <param name="message"></param>
    public void WriteLineWithMarkup(string message)
    {
        WriteWithMarkup(message);
        WriteLine("");
    }

    /// <summary>
    /// Set cursor visibility (threadsafe).
    /// </summary>
    /// <param name="visible"></param>
    public void SetCursorVisibility(bool visible)
    {
        lock (_consoleLock)
        {
            Console.CursorVisible = visible;
        }
    }

    /// <summary>
    /// Get the cursor position.
    /// </summary>
    /// <returns></returns>
    public (int Left, int Top) GetCursorPosition()
    {
        (int left, int top) = Console.GetCursorPosition();

        return (left, top);
    }

    /// <summary>
    /// Set the cursor position (threadsafe).
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    public void SetCursorPosition(int left, int top)
    {
        lock (_consoleLock)
        {
            Console.SetCursorPosition(left, top);
        }
    }
}