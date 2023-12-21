namespace ThreeXPlusOne.Code.Interfaces;

public interface IConsoleHelper
{
    void Write(string message);
    void WriteLine(string message);
    void WriteAsciiArtLogo();
    void WriteSettings();
    void WriteError(string message);
    void WriteDone();
    bool ReadYKeyToProceed(string message);
    void WriteHelpText();
    void WriteSeparator();
    void WriteHeading(string headerText);
}