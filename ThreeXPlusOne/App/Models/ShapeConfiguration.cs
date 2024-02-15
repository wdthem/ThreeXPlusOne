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
}