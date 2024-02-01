using System.Drawing;
using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.App.Models;

public record Shape()
{
    /// <summary>
    /// The type of the shape (e.g. Circle or Polygon)
    /// </summary>
    public ShapeType ShapeType { get; set; }

    /// <summary>
    /// The radius of the shape, used for both circles and polygons
    /// </summary>
    public double Radius { get; set; }

    /// <summary>
    /// A list of all the vertices of the shape, if it's a polygon
    /// </summary>
    public List<(double X, double Y)> PolygonVertices { get; set; } = [];

    /// <summary>
    /// The coordinates and radii of the ellipse (which was transformed from a circle)
    /// </summary>
    public ((double X, double Y) Center, double RadiusX, double RadiusY) EllipseConfig { get; set; }

    /// <summary>
    /// The radius and color of the node's halo when a light source exists
    /// </summary>
    public (double Radius, Color Color) HaloConfig { get; set; } = (0, Color.Empty);

    /// <summary>
    /// The colour of the shape
    /// </summary>
    public Color Color { get; set; } = Color.Empty;
}