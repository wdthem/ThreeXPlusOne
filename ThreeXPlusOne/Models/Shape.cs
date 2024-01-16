using System.Drawing;
using ThreeXPlusOne.Enums;

namespace ThreeXPlusOne.Models;

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
    public List<(float X, float Y)> Vertices { get; set; } = [];

    /// <summary>
    /// The colour of the shape
    /// </summary>
    public Color Color { get; set; } = Color.Empty;
}