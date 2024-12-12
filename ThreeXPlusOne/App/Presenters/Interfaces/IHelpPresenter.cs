namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IHelpPresenter
{
    void WriteHelpText(List<(string longName, string shortName, string description, string hint)> commandLineOptions);
    void WriteVersionText();
    void WriteCommandUsage(List<(string longName, string shortName, string description, string hint)> commandLineOptions);
}