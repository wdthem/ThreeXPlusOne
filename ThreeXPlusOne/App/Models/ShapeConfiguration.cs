namespace ThreeXPlusOne.App.Models;

public record ShapeConfiguration()
{
    /// <summary>
    /// A list of all the vertices of a polygon
    /// </summary>
    public List<(double X, double Y)> PolygonVertices { get; set; } = [];

    /// <summary>
    /// The coordinates and radii of the ellipse (which was transformed from a circle)
    /// </summary>
    public ((double X, double Y) Center, double RadiusX, double RadiusY) EllipseConfig { get; set; }

    /// <summary>
    /// The config data for drawing an arc shape
    /// </summary>
    public (double StartAngle, double SweepAngle, double Thickness) ArcConfig { get; set; }

    /// <summary>
    /// The config data for drawing a pill shape
    /// </summary>
    public (double Height, double RotationAngle, (double X, double Y) Skew) PillConfig { get; set; }

    /// <summary>
    /// The degree value of where the flat side of the semi-circle faces (0-359)
    /// </summary>
    public double SemiCircleOrientation { get; set; }
}