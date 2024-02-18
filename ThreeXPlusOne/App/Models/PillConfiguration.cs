namespace ThreeXPlusOne.App.Models;

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
    public double PillCurveRadiusX { get; set; }

    /// <summary>
    /// The y-radius of the curve of the pill shape
    /// </summary>
    public double PillCurveRadiusY { get; set; }

    /// <summary>
    /// The bounding box used to render the pill shape
    /// </summary>
    public ShapeBounds PillBounds { get; set; } = new();

    /// <summary>
    /// Skew values applied to the pill shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y)? Skew { get; set; }
}