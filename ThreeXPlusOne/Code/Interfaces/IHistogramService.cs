using ThreeXPlusOne.Code.Graph.Services;

namespace ThreeXPlusOne.Code.Interfaces;

public interface IHistogramService : IDisposable
{
    /// <summary>
    /// The graph provider implementing the interface
    /// </summary>
    GraphProvider GraphProvider { get; }

    /// <summary>
    /// Initialize the histogram canvas with the provided dimensions
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void Initialize(int width, int height);

    /// <summary>
    /// Draw the histogram
    /// </summary>
    /// <param name="counts"></param>
    /// <param name="title"></param>
    void Draw(List<int> counts, string title);

    /// <summary>
    /// Save the generated histogram image
    /// </summary>
    /// <param name="filePath"></param>
    void SaveImage(string filePath);
}