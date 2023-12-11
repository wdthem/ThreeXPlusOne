using SkiaSharp;

namespace ThreeXPlusOne.Models;

public class DirectedGraphNode
{
    public DirectedGraphNode(int value)
    {
        Value = value;
        Children = new List<DirectedGraphNode>();
        Depth = -1;
    }

    public int Value { get; set; }
    public DirectedGraphNode? Parent { get; set; }
    public List<DirectedGraphNode> Children { get; set; }
    public int Depth { get; set; }
    public SKPoint Position { get; set; }
    public bool IsPositioned { get; set; }
}