namespace ThreeXPlusOne.App.Models;

public record EllipseConfiguration
{
    /// <summary>
    /// The x-radius of the ellipse
    /// </summary>
    public double RadiusX { get; set; }

    /// <summary>
    /// The y-radius of the ellipse
    /// </summary>
    public double RadiusY { get; set; }

    /// <summary>
    /// The bounding box used to render the ellipse shape
    /// </summary>
    public ShapeBounds ShapeBounds { get; set; } = new();

    /// <summary>
    /// Skew values applied to the arc shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y)? Skew { get; set; }
}