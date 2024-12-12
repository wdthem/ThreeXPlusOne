using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;

namespace ThreeXPlusOne.App.Presenters;

public class HistogramPresenter(IConsoleService consoleService,
                                IUiComponent uiComponent) : IHistogramPresenter
{
    /// <summary>
    /// Display a message indicating that the histogram is being generated.
    /// </summary>
    public void DisplayGeneratingHistogramMessage()
    {
        consoleService.WriteWithColorMarkup("Generating histogram... ");
    }

    /// <summary>
    /// Display a message indicating that the histogram already exists.
    /// </summary>
    public void DisplayHistogramExistsMessage()
    {
        consoleService.WriteLineWithColorMarkup("[BrightJade]already exists[/]");
    }

    /// <summary>
    /// Display a message indicating that the histogram has been generated.
    /// </summary>
    public void DisplayDone()
    {
        uiComponent.WriteDone();
    }
}