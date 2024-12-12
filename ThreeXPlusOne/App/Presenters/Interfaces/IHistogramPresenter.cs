namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IHistogramPresenter
{
    /// <summary>
    /// Display a message indicating that the histogram is being generated.
    /// </summary>
    void DisplayGeneratingHistogramMessage();

    /// <summary>
    /// Display a message indicating that the histogram already exists.
    /// </summary>
    void DisplayHistogramExistsMessage();

    /// <summary>
    /// Display a message indicating that the histogram has been generated.
    /// </summary>
    void DisplayDone();
}
