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
    public (double StartAngle, double SweepAngle, double InnerRadius, double OuterRadius, (double X, double Y) Skew) ArcConfig { get; set; }

    /// <summary>
    /// The config data for drawing a pill shape
    /// </summary>
    public (double Height, double RotationAngle, (double X, double Y) Skew) PillConfig { get; set; }

    /// <summary>
    /// The config data for drawing a semicircle shape
    /// </summary>
    public (double Orientation, (double X, double Y) Skew) SemiCircleConfig { get; set; }
}