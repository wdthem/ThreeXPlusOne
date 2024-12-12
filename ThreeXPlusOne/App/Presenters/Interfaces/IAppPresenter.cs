namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IAppPresenter
{
    void DisplayAppHeader();
    void DisplayCommandParsingMessages(List<string> commandParsingMessages);
    void DisplayProcessEnd(TimeSpan elapsedTime);
}