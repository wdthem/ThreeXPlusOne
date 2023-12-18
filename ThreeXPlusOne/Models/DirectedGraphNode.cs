using SkiaSharp;

namespace ThreeXPlusOne.Models;

public class DirectedGraphNode(int value)
{
    public int Value { get; set; } = value;
    public DirectedGraphNode? Parent { get; set; }
    public List<DirectedGraphNode> Children { get; set; } = new List<DirectedGraphNode>();
    public int Depth { get; set; } = -1;
    public SKPoint Position { get; set; }
    public float Z { get; set; }
    public bool IsPositioned { get; set; }

    public bool IsFirstChild { get; set; }
    public bool IsSecondChild { get; set; }

    public float Radius { get; set; }
}