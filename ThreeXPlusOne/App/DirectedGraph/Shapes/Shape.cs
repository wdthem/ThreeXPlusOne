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
    /// <returns></returns>
    protected void GenerateShapeSkew()
    {
        double skewFactor = (Random.Shared.NextDouble() > 0.5 ? 1 : -1) * ((0.1 + Random.Shared.NextDouble()) * 0.6);
        double skewX = skewFactor;
        double skewY = skewX * Random.Shared.NextDouble();

        _shapeConfiguration.Skew = (skewX, skewY);
    }

    /// <summary>
    /// Rotate the points so the shape is not always drawn in the same orientation
    /// </summary>
    /// <param name="point"></param>
    /// <param name="center"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    protected static (double X, double Y) RotateVertex((double X, double Y) point,
                                                       (double X, double Y) center,
                                                       double angle)
    {
        double cosAngle = Math.Cos(angle);
        double sinAngle = Math.Sin(angle);

        double x = cosAngle * (point.X - center.X) - sinAngle * (point.Y - center.Y) + center.X;
        double y = sinAngle * (point.X - center.X) + cosAngle * (point.Y - center.Y) + center.Y;

        return (x, y);
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