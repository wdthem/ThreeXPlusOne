using SkiaSharp;

namespace ThreeXPlusOne.App.Models;

public record SkiaSharpShapeRenderContext
{
    /// <summary>
    /// The canvas on which the shape is being rendered.
    /// </summary>
    public required SKCanvas Canvas { get; set; }

    /// <summary>
    /// The node being rendered.
    /// </summary>
    public required DirectedGraphNode Node { get; set; }

    /// <summary>
    /// The paint applied to the node.
    /// </summary>
    public required SKPaint Paint { get; set; }

    /// <summary>
    /// The border paint applied to the node.
    /// </summary>
    public required SKPaint BorderPaint { get; set; }
}