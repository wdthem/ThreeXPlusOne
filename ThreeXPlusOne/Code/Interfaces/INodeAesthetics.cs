using System.Drawing;
using ThreeXPlusOne.Code.Models;

namespace ThreeXPlusOne.Code.Interfaces;

public interface INodeAesthetics
{
    /// <summary>
    /// Assign a ShapeType to the node and vertices if applicable
    /// </summary>
    /// <param name="node"></param>
    /// <param name="random"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="includePolygonsAsNodes"></param>
    void SetNodeShape(DirectedGraphNode node,
                      Random random,
                      double nodeRadius,
                      bool includePolygonsAsNodes);

    /// <summary>
    /// Rotate a node's x,y coordinate position based on whether the node's integer value is even or odd
    /// If even, rotate clockwise. If odd, rotate anti-clockwise. But if the coordinates are in negative space, reverse this.
    /// </summary>
    /// <param name="nodeValue"></param>
    /// <param name="rotationAngle"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    (double X, double Y) RotateNode(int nodeValue,
                                    double rotationAngle,
                                    double x,
                                    double y);

    /// <summary>
    /// Generate a random colour for the node
    /// </summary>
    /// <param name="random"></param>
    /// <returns></returns>
    Color GenerateNodeColor(Random random);

    /// <summary>
    /// If a light source is in place, it should impact the colour of nodes.
    /// The closer to the source, the more the impact.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nodeBaseColor"></param>
    /// <param name="lightSourceCoordinates"></param>
    /// <param name="lightSourceMaxDistanceEffect"></param>
    /// <param name="lightSourceColor"></param>
    /// <returns></returns>
    void ApplyLightSourceToNode(DirectedGraphNode node,
                                Color nodeBaseColor,
                                (double X, double Y) lightSourceCoordinates,
                                double lightSourceMaxDistanceEffect,
                                Color lightSourceColor);
}