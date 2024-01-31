namespace ThreeXPlusOne.App.Models;

public class DirectedGraphNode(int numberValue)
{
    /// <summary>
    /// The number value of the node from the series of generated numbers
    /// </summary>
    public int NumberValue { get; set; } = numberValue;

    /// <summary>
    /// The node's parent node
    /// </summary>
    public DirectedGraphNode? Parent { get; set; }

    /// <summary>
    /// The node's child nodes (max of 2)
    /// </summary>
    public List<DirectedGraphNode> Children { get; set; } = [];

    /// <summary>
    /// The depth of this node on the graph. When running the algorithm on many numbers, there can be many numbers at any given depth.
    /// </summary>
    public int Depth { get; set; } = -1;

    /// <summary>
    /// The x,y position of the node on the graph
    /// </summary>
    public (double X, double Y) Position { get; set; }

    /// <summary>
    /// The Z coordinate of the node for use in pseudo 3D graphs
    /// </summary>
    public double Z { get; set; }

    /// <summary>
    /// Whether or not the node's coordinates have been calculated
    /// </summary>
    public bool IsPositioned { get; set; }

    /// <summary>
    /// Whether or not this is the first child node of the given parent node
    /// </summary>
    /// <remarks>A node can only have two child nodes</remarks>
    public bool IsFirstChild { get; set; }

    /// <summary>
    /// An object to store information about the node's rendered shape
    /// </summary>
    public Shape Shape { get; set; } = new Shape();
}