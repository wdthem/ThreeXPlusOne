namespace ThreeXPlusOne.Code.Interfaces;

public interface IConsoleHelper
{
    /// <summary>
    /// Issue a Console.Write() command
    /// </summary>
    /// <param name="message"></param>
    void Write(string message);

    /// <summary>
    /// Issue a Console.WriteLine() command
    /// </summary>
    /// <param name="message"></param>
    void WriteLine(string message);

    /// <summary>
    /// Output the settings from the settings file, or defaults
    /// </summary>
    void WriteSettings();

    /// <summary>
    /// Output a message regarding whether the user saved the settings file or not
    /// </summary>
    /// <param name="savedSettings"></param>
    void WriteSettingsSavedMessage(bool savedSettings);

    /// <summary>
    /// Output an error message
    /// </summary>
    /// <param name="message"></param>
    void WriteError(string message);

    /// <summary>
    /// Output an indicator that a given task is complete
    /// </summary>
    void WriteDone();

    /// <summary>
    /// Prompt the user to press y or n on the keyboard
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    bool ReadYKeyToProceed(string message);

    /// <summary>
    /// Output a separator line
    /// </summary>
    void WriteSeparator();

    /// <summary>
    /// Output a section heading
    /// </summary>
    /// <param name="headerText"></param>
    void WriteHeading(string headerText);

    /// <summary>
    /// Output the help text for the app
    /// </summary>
    void WriteHelpText();

    /// <summary>
    /// Output the 3x+1 console app ascii art logo
    /// </summary>
    void WriteAsciiArtLogo();

    /// <summary>
    /// Output a visual indication that the process is doing work
    /// </summary>
    /// <param name="token"></param>
    void WriteSpinner(CancellationToken token);
}