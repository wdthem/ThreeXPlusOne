namespace ThreeXPlusOne.App.Presenters.Interfaces.Components;

public interface IUiComponent
{
    /// <summary>
    /// Write an error message.
    /// </summary>
    /// <param name="message"></param>
    void WriteError(string message);

    /// <summary>
    /// Write a message indicating the given step completed.
    /// </summary>
    void WriteDone();

    /// <summary>
    /// Read the y or n key press by the user to know whether or not to proceed.
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    bool AskForConfirmation(string prompt);

    /// <summary>
    /// Write a visual separator in console output.
    /// </summary>
    void WriteSeparator();

    /// <summary>
    /// Write a section heading in console output.
    /// </summary>
    /// <param name="headerText"></param>
    void WriteHeading(string headerText);
}