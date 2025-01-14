using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services.Interfaces;

public interface IDirectedGraphDrawingService : IScopedService, IDisposable
{
    /// <summary>
    /// Action to perform when the service starts a task.
    /// </summary>
    Action<string>? OnStart { get; set; }

    /// <summary>
    /// Action to perform when the service ends a task.
    /// </summary>
    Action<string?>? OnComplete { get; set; }

    /// <summary>
    /// The graph provider implementing the interface.
    /// </summary>
    GraphProvider GraphProvider { get; }

    /// <summary>
    /// Initialize the graph based on the provided dimensions.
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="backgroundColor"></param>
    void Initialize(List<DirectedGraphNode> nodes, int width, int height, Color backgroundColor);

    /// <summary>
    /// Add a light source to the graph emanating from the supplied coordinates.
    /// </summary>
    /// <param name="lightSourceCoordinates"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <exception cref="Exception"></exception>
    void GenerateLightSource((double X, double Y) lightSourceCoordinates, double radius, Color color);

    /// <summary>
    /// Draw the graph based on provided app settings.
    /// </summary>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="drawNodeConnections"></param>
    /// <exception cref="Exception"></exception>
    void Draw(bool drawNumbersOnNodes, bool drawNodeConnections);

    /// <summary>
    /// Save the generated graph as the file type specified in the app settings.
    /// </summary>`
    /// <param name="imageTypeAppSetting"></param>
    /// <param name="imageQualityAppSetting"></param>
    /// <exception cref="Exception"></exception>
    void SaveImage(string imageTypeAppSetting,
                   int imageQualityAppSetting);
}