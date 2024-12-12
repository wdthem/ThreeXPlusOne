using System.Text.RegularExpressions;
using ThreeXPlusOne.App.Helpers;
using ThreeXPlusOne.App.Services.Base;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Services;

public partial class ConsoleService : ConsoleServiceBase, IConsoleService
{
    private static readonly object _consoleLock = new();

    [GeneratedRegex(@"(\x1b[\[\]](?:[^\x07\\\]]*[\\\]]{0,1})*(?:\x07|\x1b\\)?)|(\[/\]|\[/?[^\]]+\]|[^\[\]\x1b]+)")]
    private static partial Regex ColorMarkup();

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
    /// Write a message to the console with color markup via ANSI escape codes for full color support.
    /// </summary>
    /// <param name="message"></param>
    public void WriteWithColorMarkup(string message)
    {
        foreach (Match match in ColorMarkup().Matches(message))
        {
            if (!match.Value.StartsWith('['))
            {
                Write(match.Value);
                continue;
            }

            string value = match.Value.Trim('[', ']');
            Write(value.StartsWith('/')
                ? ColorHelper.DefaultAnsiForegroundColour
                : ColorHelper.GetHexColor(value) is string hex
                    ? ColorHelper.HexColorToAnsiForegroundColorString(hex)
                    : match.Value);
        }
    }

    /// <summary>
    /// Write an API action message to the console with color markup as a single line.
    /// </summary>
    /// <param name="message"></param>
    public void WriteLineWithColorMarkup(string message)
    {
        WriteWithColorMarkup(message);
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