namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record PillConfiguration
{
    /// <summary>
    /// The height of the pill shape
    /// </summary>
    /// <remarks>
    /// Width is determined by the node radius
    /// </remarks>
    public double Height { get; set; }

    /// <summary>
    /// The angle by which the shape is rotated on the graph
    /// </summary>
    public double RotationAngle { get; set; }

    /// <summary>
    /// The x-radius of the curve of the pill shape
    /// </summary>
    public double CurveRadiusX { get; set; }

    /// <summary>
    /// The y-radius of the curve of the pill shape
    /// </summary>
    public double CurveRadiusY { get; set; }

    /// <summary>
    /// The bounding box used to render the pill shape
    /// </summary>
    public ShapeBounds ShapeBounds { get; set; } = new();
}