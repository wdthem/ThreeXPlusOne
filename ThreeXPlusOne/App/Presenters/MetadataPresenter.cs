using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;

namespace ThreeXPlusOne.App.Presenters;

public class MetadataPresenter(IConsoleService consoleService,
                               IUiComponent uiComponent) : IMetadataPresenter
{
    /// <summary>
    /// Display a message indicating that the metadata is being generated.
    /// </summary>
    public void DisplayGeneratingMetadataMessage()
    {
        consoleService.WriteWithColorMarkup("Generating metadata... ");
    }

    /// <summary>
    /// Display a message indicating that the number series metadata is being generated.
    /// </summary>
    public void DisplayGeneratingNumberSeriesMetadataMessage()
    {
        consoleService.WriteWithColorMarkup("Generating number series metadata... ");
    }

    /// <summary>
    /// Display a message indicating that the metadata already exists.
    /// </summary>
    public void DisplayMetadataExistsMessage()
    {
        consoleService.WriteLineWithColorMarkup("[BrightJade]already exists[/]");
    }

    /// <summary>
    /// Display a message indicating that the metadata has been generated.
    /// </summary>
    public void DisplayDone()
    {
        uiComponent.WriteDone();
    }
}