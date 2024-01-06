using SkiaSharp;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Interfaces;

public interface IGraphService
{
    void InitializeGraph(int width, int height);

    /// <summary>
    /// Draw the lines connecting nodes to their parent/children
    /// </summary>
    /// <param name="node"></param>
    void DrawConnection(DirectedGraphNode node);
    void DrawNode(DirectedGraphNode node, bool drawNumbersOnNodes, bool distortNodes, int radiusDistortion);
    void GenerateBackgroundStars(int count);
    void SaveGraphImage();
}