using System.Drawing;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public abstract class Shape()
{
    /// <summary>
    /// The object holding the configuration details required for rendering a given shape
    /// </summary>
    protected readonly ShapeConfiguration _shapeConfiguration = new();

    /// <summary>
    /// Generate skew data for the given shape
    /// </summary>
    /// <param name="skewFactor"></param>
    /// <returns></returns>
    protected static (double X, double Y)? GetShapeSkew(double? skewFactor)
    {
        (double X, double Y)? skew = null;

        if (skewFactor != null && skewFactor != 0)
        {
            double skewX = skewFactor.Value;
            double skewY = skewX * Random.Shared.NextDouble();

            skew = (skewX, skewY);
        }

        return skew;
    }

    /// <summary>
    /// The colour of the shape's border
    /// </summary>
    public Color BorderColor { get; set; } = Color.Empty;

    /// <summary>
    /// The colour of the shape
    /// </summary>
    public Color Color { get; set; } = Color.Empty;

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

    /// <summary>
    /// Set the halo configuration for the shape
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    public void SetNodeHaloConfiguration(double radius, Color color)
    {
        _shapeConfiguration.HaloConfiguration = (radius, color);
    }
}