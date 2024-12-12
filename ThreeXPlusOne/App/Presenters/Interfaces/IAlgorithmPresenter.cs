namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IAlgorithmPresenter
{
    void DisplayHeading(string heading);
    void DisplayRunningAlgorithmMessage(int numberCount);
    void DisplayUsingSeriesMessage();
    void DisplayGeneratingRandomNumbersMessage();
    void DisplayGaveUpGeneratingNumbersMessage(int numberCount);
    void DisplayDone();
}