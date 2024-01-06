using ThreeXPlusOne.Code.Graph.GraphProviders;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Interfaces;

public interface IGraphService
{
    /// <summary>
    /// The graph provider implementing the interface
    /// </summary>
    GraphProvider GraphProvider { get; }

    /// <summary>
    /// Initialize the graph based on the provided dimensions
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    void InitializeGraph(int width, int height);

    /// <summary>
    /// Draw the lines connecting nodes to their parent/children
    /// </summary>
    /// <param name="node"></param>
    void DrawConnection(DirectedGraphNode node);

    /// <summary>
    /// Draw the node at its defined position
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    void DrawNode(DirectedGraphNode node, bool drawNumbersOnNodes, bool distortNodes);

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="count"></param>
    void GenerateBackgroundStars(int count);

    /// <summary>
    /// Save the generated graph as a png
    /// </summary>
    /// <exception cref="Exception"></exception>
    void SaveGraphImage();
}