using System.Collections.ObjectModel;
using System.Drawing;
using ThreeXPlusOne.Code.Enums;
using ThreeXPlusOne.Code.Models;

namespace ThreeXPlusOne.Code.Interfaces;

public interface IDirectedGraphService : IDisposable
{
    /// <summary>
    /// Action to perform when the service starts a task
    /// </summary>
    Action<string>? OnStart { get; set; }

    /// <summary>
    /// Action to perform when the service ends a task
    /// </summary>
    Action? OnComplete { get; set; }

    /// <summary>
    /// The graph provider implementing the interface
    /// </summary>
    GraphProvider GraphProvider { get; }

    /// <summary>
    /// The dimensions the given graph service implementation supports in the context of rendering the graph
    /// </summary>
    ReadOnlyCollection<int> SupportedDimensions { get; }

    /// <summary>
    /// Initialize the graph based on the provided dimensions
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="backgroundColor"></param>
    void Initialize(List<DirectedGraphNode> nodes, int width, int height, Color backgroundColor);

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="starCount"></param>
    /// <exception cref="Exception"></exception>
    void GenerateBackgroundStars(int starCount);

    /// <summary>
    /// Add a light source to the graph emanating from the supplied coordinates
    /// </summary>
    /// <param name="lightSourceCoordinates"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <exception cref="Exception"></exception>
    void GenerateLightSource((float X, float Y) lightSourceCoordinates, float radius, Color color);

    /// <summary>
    /// Draw the graph based on provided settings
    /// </summary>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="drawNodeConnections"></param>
    /// <exception cref="Exception"></exception>
    void Draw(bool drawNumbersOnNodes, bool drawNodeConnections);

    /// <summary>
    /// Render the graph
    /// </summary>
    /// <exception cref="Exception"></exception>
    void Render();

    /// <summary>
    /// Save the generated graph as a png
    /// </summary>
    /// <exception cref="Exception"></exception>
    void SaveImage();
}