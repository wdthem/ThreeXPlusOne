using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class MetadataPresenter(IConsoleService consoleService,
                               IUiComponent uiComponent) : IMetadataPresenter
{
    /// <summary>
    /// Display a message indicating that the metadata is being generated.
    /// </summary>
    public void DisplayMetadataHeader()
    {
        uiComponent.WriteHeading("Metadata");
    }

    /// <summary>
    /// Display a message indicating that the number series metadata is being generated.
    /// </summary>
    public void DisplayGeneratingNumberSeriesMetadataMessage()
    {
        consoleService.WriteWithMarkup("  Generating number series metadata... ");
    }

    /// <summary>
    /// Display a message indicating that the metadata already exists.
    /// </summary>
    public void DisplayMetadataExistsMessage()
    {
        consoleService.WriteLineWithMarkup("[BrightJade]already exists[/]");
    }

    /// <summary>
    /// Display a message indicating that the metadata has been generated.
    /// </summary>
    public void DisplayDone()
    {
        uiComponent.WriteDone();
    }
}