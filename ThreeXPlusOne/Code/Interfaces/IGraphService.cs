using System.Collections.ObjectModel;
using ThreeXPlusOne.Code.Graph.Services;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Interfaces;

public interface IGraphService : IDisposable
{
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
    void Initialize(List<DirectedGraphNode> nodes, int width, int height);

    /// <summary>
    /// Draw the graph based on provided settings
    /// </summary>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="distortNodes"></param>
    /// <param name="drawConnections"></param>
    void Draw(bool drawNumbersOnNodes, bool distortNodes, bool drawConnections);

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="starCount"></param>
    void GenerateBackgroundStars(int starCount);

    /// <summary>
    /// Render the graph
    /// </summary>
    void Render();

    /// <summary>
    /// Save the generated graph as a png
    /// </summary>
    /// <exception cref="Exception"></exception>
    void SaveImage();
}