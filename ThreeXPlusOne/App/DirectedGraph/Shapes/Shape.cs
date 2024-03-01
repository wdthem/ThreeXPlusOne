using System.Drawing;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public abstract class Shape()
{
    private Color _threeDimensionalSideGradientStartColor = Color.Empty;

    private Color _threeDimensionalSideGradientEndColor = Color.Empty;

    /// <summary>
    /// The object holding the configuration details required for rendering a given shape
    /// </summary>
    protected readonly ShapeConfiguration _shapeConfiguration = new();

    /// <summary>
    /// Generate a skewed position for the given shape
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
    /// The color to use to start the gradient on the side of the shape in pseudo-3D
    /// </summary>
    public Color ThreeDimensionalSideGradientStartColor
    {
        get
        {
            if (_threeDimensionalSideGradientStartColor == Color.Empty)
            {
                _threeDimensionalSideGradientStartColor = Color.FromArgb(200,
                                                                         Color.R,
                                                                         Color.G,
                                                                         Color.B);
            }

            return _threeDimensionalSideGradientStartColor;
        }
        set
        {
            _threeDimensionalSideGradientStartColor = value;
        }
    }

    /// <summary>
    /// The color to use to end the gradient on the side of the shape in pseudo-3D
    /// </summary>
    public Color ThreeDimensionalSideGradientEndColor
    {
        get
        {
            if (_threeDimensionalSideGradientEndColor == Color.Empty)
            {
                float factor = 0.6f;

                int r = (int)Math.Clamp(Color.R * factor, 0, 255);
                int g = (int)Math.Clamp(Color.G * factor, 0, 255);
                int b = (int)Math.Clamp(Color.B * factor, 0, 255);

                _threeDimensionalSideGradientEndColor = Color.FromArgb(Color.A, r, g, b);
            }

            return _threeDimensionalSideGradientEndColor;
        }
    }

    /// <summary>
    /// Get the shape's configuration data
    /// </summary>
    /// <returns></returns>
    public ShapeConfiguration GetShapeConfiguration()
    {
        return _shapeConfiguration;
    }

    /// <summary>
    /// Set the start and end of the gradient of the sides of the 3D shape
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    public void SetNodeThreeDimensionalGradientPoints((double X, double Y) startPoint,
                                                      (double X, double Y) endPoint)
    {
        _shapeConfiguration.ThreeDimensionalSideGradientStartPoint = startPoint;
        _shapeConfiguration.ThreeDimensionalSideGradientEndPoint = endPoint;
    }
}