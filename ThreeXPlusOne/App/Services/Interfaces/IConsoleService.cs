namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IConsoleService : ISingletonService
{
    /// <summary>
    /// Get the width of the console window.
    /// </summary>
    int AppConsoleWidth { get; }

    /// <summary>
    /// Issue a Console.Write() command.
    /// </summary>
    /// <param name="message"></param>
    void Write(string message);

    /// <summary>
    /// Issue a Console.WriteLine() command.
    /// </summary>
    /// <param name="message"></param>
    void WriteLine(string message);

    /// <summary>
    /// Write a message to the console with color markup via ANSI escape codes for full color support.
    /// </summary>
    /// <param name="message"></param>
    void WriteWithColorMarkup(string message);

    /// <summary>
    /// Write a message to the console with color markup via ANSI escape codes for full color support.
    /// </summary>
    /// <param name="message"></param>
    void WriteLineWithColorMarkup(string message);

    /// <summary>
    /// Set the visibility of the cursor.
    /// </summary>
    /// <param name="visible"></param>
    void SetCursorVisibility(bool visible);

    /// <summary>
    /// Get the cursor position.
    /// </summary>
    /// <returns></returns>
    (int Left, int Top) GetCursorPosition();

    /// <summary>
    /// Set the cursor position.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    void SetCursorPosition(int left, int top);
}