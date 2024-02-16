using System.Drawing;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public abstract class Shape()
{
    /// <summary>
    /// The object holding the configuration details required for rendering a given shape
    /// </summary>
    protected readonly ShapeConfiguration _shapeConfiguration = new();

    /// <summary>
    /// The colour of the shape's border
    /// </summary>
    public Color BorderColor { get; set; } = Color.Empty;

    /// <summary>
    /// The colour of the shape
    /// </summary>
    public Color Color { get; set; } = Color.Empty;

    /// <summary>
    /// The radius and colour of the node's halo when a light source exists
    /// </summary>
    public (double Radius, Color Color) HaloConfig { get; set; } = (0, Color.Empty);

    /// <summary>
    /// The radius of the shape
    /// </summary>
    public double Radius { get; set; }

    /// <summary>
    /// Get the shape's configuration data
    /// </summary>
    /// <returns></returns>
    public ShapeConfiguration GetShapeConfiguration()
    {
        return _shapeConfiguration;
    }
}