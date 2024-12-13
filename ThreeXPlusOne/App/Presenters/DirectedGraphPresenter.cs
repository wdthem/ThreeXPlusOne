using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Helpers;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class DirectedGraphPresenter(IConsoleService consoleService,
                                    IProgressIndicatorPresenter progressIndicatorPresenter,
                                    IUiComponent uiComponent) : IDirectedGraphPresenter
{
    /// <summary>
    /// Display a message indicating that the series are being added to the graph.
    /// </summary>
    /// <param name="seriesCount"></param>
    public void DisplayAddingSeriesMessage(int seriesCount)
    {
        consoleService.WriteWithColorMarkup($"  Adding {seriesCount} series to the graph... ");
    }

    /// <summary>
    /// Display a message indicating that the canvas dimensions are being set.
    /// </summary>
    /// <param name="canvasWidth"></param>
    /// <param name="canvasHeight"></param>
    public void DisplaySettingCanvasSizeMessage(int canvasWidth, int canvasHeight)
    {
        consoleService.WriteWithColorMarkup($"  Setting canvas dimensions to {canvasWidth:N0} x {canvasHeight:N0}... ");
    }

    /// <summary>
    /// Display a message indicating that the nodes are being positioned.
    /// </summary>
    /// <param name="nodesPositioned"></param>
    public void DisplayNodesPositionedMessage(int nodesPositioned)
    {
        consoleService.WriteWithColorMarkup($"\r  {nodesPositioned} nodes positioned... ");
    }

    /// <summary>
    /// Display a message indicating that the nodes are being styled.
    /// </summary>
    /// <param name="nodesStyled"></param>
    public void DisplayNodesStyledMessage(int nodesStyled)
    {
        consoleService.WriteWithColorMarkup($"\r  {nodesStyled} nodes styled... ");
    }

    /// <summary>
    /// Write an action message.
    /// </summary>
    /// <param name="message"></param>
    public void WriteActionMessage(string message)
    {
        consoleService.WriteWithColorMarkup(message);
    }

    /// <summary>
    /// Display a heading.
    /// </summary>
    /// <param name="heading"></param>
    public void DisplayHeading(string heading)
    {
        uiComponent.WriteHeading(heading);
    }

    /// <summary>
    /// Get confirmation from the user.
    /// </summary>
    /// <param name="message"></param>
    public bool GetConfirmation(string message)
    {
        uiComponent.WriteSeparator();

        bool result = uiComponent.AskForConfirmation(message);

        uiComponent.WriteSeparator();

        return result;
    }

    /// <summary>
    /// Display a message indicating that the graph generation was cancelled.
    /// </summary>
    public void DisplayGraphGenerationCancelledMessage()
    {
        consoleService.WriteLineWithColorMarkup($"  {EmojiHelper.GetEmojiUnicodeValue(Emoji.RedX)} Graph generation cancelled");
    }

    /// <summary>
    /// Display a message indicating that the series have been added to the graph.
    /// </summary>
    public void DisplayDone()
    {
        uiComponent.WriteDone();
    }

    /// <summary>
    /// Display a progress indicator.
    /// </summary>
    /// <param name="message"></param>
    public async Task DisplayProgressIndicator(string? message = null)
    {
        await progressIndicatorPresenter.StartSpinner(message);
    }

    /// <summary>
    /// Stop the progress indicator.
    /// </summary>
    /// <param name="message"></param>
    public async Task StopProgressIndicator(string? message = null)
    {
        await progressIndicatorPresenter.StopSpinner();
    }
}
