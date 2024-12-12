namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IDirectedGraphPresenter
{
    /// <summary>
    /// Display a message indicating that the series are being added to the graph.
    /// </summary>
    /// <param name="seriesCount"></param>
    void DisplayAddingSeriesMessage(int seriesCount);

    /// <summary>
    /// Display a message indicating that the canvas dimensions are being set.
    /// </summary>
    /// <param name="canvasWidth"></param>
    /// <param name="canvasHeight"></param>
    void DisplaySettingCanvasSizeMessage(int canvasWidth, int canvasHeight);

    /// <summary>
    /// Display a message indicating that the nodes are being positioned.
    /// </summary>
    /// <param name="nodesPositioned"></param>
    void DisplayNodesPositionedMessage(int nodesPositioned);

    /// <summary>
    /// Display a message indicating that the nodes are being styled.
    /// </summary>
    /// <param name="nodesStyled"></param>
    void DisplayNodesStyledMessage(int nodesStyled);

    /// <summary>
    /// Display a heading.
    /// </summary>
    /// <param name="heading"></param>
    void DisplayHeading(string heading);

    /// <summary>
    /// Get confirmation from the user.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    bool GetConfirmation(string message);

    /// <summary>
    /// Display a message indicating that the graph generation was cancelled.
    /// </summary>
    void DisplayGraphGenerationCancelledMessage();

    /// <summary>
    /// Write an action message.
    /// </summary>
    /// <param name="message"></param>
    void WriteActionMessage(string message);

    /// <summary>
    /// Display a message indicating that the series have been added to the graph.
    /// </summary>
    void DisplayDone();

    /// <summary>
    /// Display a progress indicator.
    /// </summary>
    /// <param name="message"></param>
    Task DisplayProgressIndicator(string? message = null);

    /// <summary>
    /// Stop the progress indicator.
    /// </summary>
    /// <param name="message"></param>
    Task StopProgressIndicator(string? message = null);
}
