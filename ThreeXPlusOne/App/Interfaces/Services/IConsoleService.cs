namespace ThreeXPlusOne.App.Interfaces.Services;

public interface IConsoleService : ISingletonService
{
    /// <summary>
    /// Issue a Console.Write() command.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="delay"></param>
    void Write(string message, bool delay = false);

    /// <summary>
    /// Issue a Console.WriteLine() command.
    /// </summary>
    /// <param name="message"></param>
    void WriteLine(string message);

    /// <summary>
    /// Set the out color of the console.
    /// </summary>
    /// <param name="color"></param>
    void SetForegroundColor(ConsoleColor color);

    /// <summary>
    /// Set the visibility of the cursor.
    /// </summary>
    /// <param name="visible"></param>
    void SetCursorVisibility(bool visible);

    /// <summary>
    /// Set the cursor position.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    void SetCursorPosition(int left, int top);

    /// <summary>
    /// Output the app settings from the app settings file, or defaults.
    /// </summary>
    /// <remarks>
    /// Optional properties to support recursive calls through the nested AppSettings classes.
    /// </remarks>
    /// <param name="type"></param>
    /// <param name="instance"></param>
    /// <param name="sectionName"></param>
    /// <param name="includeHeader"></param>
    /// <param name="isJson"></param>
    void WriteSettings(Type? type = null, object? instance = null, string? sectionName = null, bool includeHeader = true, bool isJson = false);

    /// <summary>
    /// Output any non-breaking messages that occurred during command-line parsing.
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    void WriteCommandParsingMessages(List<string> commandParsingMessages);

    /// <summary>
    /// Output a message regarding whether the user saved the app settings file or not.
    /// </summary>
    /// <param name="savedSettings"></param>
    void WriteSettingsSavedMessage(bool savedSettings);

    /// <summary>
    /// Output an error message.
    /// </summary>
    /// <param name="message"></param>
    void WriteError(string message);

    /// <summary>
    /// Output an indicator that a given task is complete.
    /// </summary>
    void WriteDone();

    /// <summary>
    /// Prompt the user to press y or n on the keyboard.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    bool ReadYKeyToProceed(string message);

    /// <summary>
    /// Output a separator line.
    /// </summary>
    void WriteSeparator();

    /// <summary>
    /// Output a section heading.
    /// </summary>
    /// <param name="headerText"></param>
    void WriteHeading(string headerText);

    /// <summary>
    /// Output the general help text for the app.
    /// </summary>
    /// <param name="commandLineOptions"></param>
    void WriteHelpText(List<(string longName, string shortName, string description, string hint)> commandLineOptions);

    /// <summary>
    /// Output the command help.
    /// </summary>
    /// <param name="commandLineOptions"></param>
    void WriteCommandUsage(List<(string longName, string shortName, string description, string hint)> commandLineOptions);

    /// <summary>
    /// Output the configuration info for the app.
    /// </summary>
    void WriteConfigText();

    /// <summary>
    /// Output the current version of the app.
    /// </summary>
    void WriteVersionText();

    /// <summary>
    /// Output the 3x+1 console app ascii art logo.
    /// </summary>
    void WriteAsciiArtLogo();

    /// <summary>
    /// Output a message to end the process, with details about the total execution time.
    /// </summary>
    /// <param name="timespan"></param>
    void WriteProcessEnd(TimeSpan timespan);

    /// <summary>
    /// Output a visual indication that the process is doing work.
    /// </summary>
    /// <param name="token"></param>
    void ShowSpinningBar();

    /// <summary>
    /// Stop the spinning bar.
    /// </summary>
    void StopSpinningBar();
}