using System.Drawing;
using ThreeXPlusOne.Code.Enums;

namespace ThreeXPlusOne.Code.Models;

public class Shape()
{
    /// <summary>
    /// The type of the shape (e.g. Circle or Polygon)
    /// </summary>
    public ShapeType ShapeType { get; set; }

    /// <summary>
    /// The radius of the shape, used for both circles and polygons
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// A list of all the vertices of the shape, if it's a polygon
    /// </summary>
    public List<(float X, float Y)> PolygonVertices { get; set; } = [];

    /// <summary>
    /// The coordinates and radii of the ellipse (which was transformed from a circle)
    /// </summary>
    public ((float X, float Y) Center, float RadiusX, float RadiusY) EllipseConfig { get; set; }

    /// <summary>
    /// An object to store light ray data for rays of light around nodes close to the light source
    /// </summary>
    public List<((float X, float Y) Start, (float X, float Y) End, Color Color)> LightRayData { get; set; } = [];

    /// <summary>
    /// The colour of the shape
    /// </summary>
    public Color Color { get; set; } = Color.Empty;
}