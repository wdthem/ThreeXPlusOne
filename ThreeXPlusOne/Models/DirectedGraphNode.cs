using SkiaSharp;

namespace ThreeXPlusOne.Models;

public class DirectedGraphNode(int value)
{
    /// <summary>
    /// The number value of the node from the series of generated numbers
    /// </summary>
    public int Value { get; set; } = value;

    /// <summary>
    /// The node's parent node
    /// </summary>
    public DirectedGraphNode? Parent { get; set; }

    /// <summary>
    /// The nodes child nodes (max of 2)
    /// </summary>
    public List<DirectedGraphNode> Children { get; set; } = [];

    /// <summary>
    /// The depth of this node on the graph. When running the algorithm on many numbers, there can be many numbers at any given depth.
    /// </summary>
    public int Depth { get; set; } = -1;

    /// <summary>
    /// The x,y position of the node on the graph, as a SkiaSharp SKPoint
    /// </summary>
    public SKPoint Position { get; set; }

    /// <summary>
    /// The Z coordinate of the node for use in pseudo 3D graphs
    /// </summary>
    public float Z { get; set; }

    /// <summary>
    /// Whether or not the node's coordinates has been calculated
    /// </summary>
    public bool IsPositioned { get; set; }

    /// <summary>
    /// Whether or not this is the first child node of the given parent node
    /// </summary>
    public bool IsFirstChild { get; set; }

    /// <summary>
    /// Whether or not this is the second child node of the given parent node
    /// </summary>
    public bool IsSecondChild { get; set; }

    /// <summary>
    /// The radius of the node
    /// </summary>
    public float Radius { get; set; }
}