using System.Drawing;

namespace ThreeXPlusOne.App.Models.Shapes;

public abstract class Shape()
{
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
}