namespace ThreeXPlusOne.App.Presenters.Interfaces;

public interface IMetadataPresenter
{
    /// <summary>
    /// Display a message indicating that the metadata is being generated.
    /// </summary>
    void DisplayGeneratingMetadataMessage();

    /// <summary>
    /// Display a message indicating that the number series metadata is being generated.
    /// </summary>
    void DisplayGeneratingNumberSeriesMetadataMessage();

    /// <summary>
    /// Display a message indicating that the metadata already exists.
    /// </summary>
    void DisplayMetadataExistsMessage();

    /// <summary>
    /// Display a message indicating that the metadata has been generated.
    /// </summary>
    void DisplayDone();
}