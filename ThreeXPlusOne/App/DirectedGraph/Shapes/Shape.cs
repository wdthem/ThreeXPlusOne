using System.Drawing;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

/// <summary>
/// The base class for all Shape instances.
/// </summary>
public abstract class Shape()
{
    private Color _gradientStartColor = Color.Empty;

    private Color _gradientEndColor = Color.Empty;

    private Color _borderGradientStartColor = Color.Empty;

    private Color _borderGradientEndColor = Color.Empty;

    private readonly double _threeDimensionalDepthMultiplier = 0.1;

    private readonly int _threeDimensionalSideCount = 360;

    /// <summary>
    /// Skew values applied to the shape in psuedo-3D graphs.
    /// </summary>
    public (double X, double Y)? Skew { get; set; }

    /// <summary>
    /// The number of sides to render when drawing the shape in pseudo-3D.
    /// </summary>
    public int ThreeDimensionalSideCount => _threeDimensionalSideCount;

    /// <summary>
    /// The dimensions this shape is to be rendered in.
    /// </summary>
    /// <remarks>
    /// Only 3D shapes can have skew.
    /// </remarks>
    public int Dimensions => Skew == null ? 2 : 3;

    /// <summary>
    /// The start point of the gradient for the shape's side.
    /// </summary>
    public (double X, double Y) SideFaceGradientStartPoint { get; set; }

    /// <summary>
    /// The end point of the gradient for the shape's side.
    /// </summary>
    public (double X, double Y) SideFaceGradientEndPoint { get; set; }

    /// <summary>
    /// The start point of the gradient for the shape's front face.
    /// </summary>
    public (double X, double Y) FrontFaceGradientStartPoint { get; set; }

    /// <summary>
    /// The end point of the gradient for the shape's front face.
    /// </summary>
    public (double X, double Y) FrontFaceGradientEndPoint { get; set; }

    /// <summary>
    /// Generate a skewed position for the given shape.
    /// </summary>
    /// <returns></returns>
    protected void GenerateShapeSkew()
    {
        double skewFactor = (Random.Shared.NextDouble() > 0.5 ? 1 : -1) * ((0.1 + Random.Shared.NextDouble()) * 0.6);
        double skewX = skewFactor;
        double skewY = skewX * Random.Shared.NextDouble();

        Skew = (skewX, skewY);
    }

    /// <summary>
    /// Rotate the points so the shape is not always drawn in the same orientation.
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
    /// The depth of the shape when rendered in pseudo-3D, based on the node radius.
    /// </summary>
    /// <param name="nodeRadius"></param>
    /// <returns></returns>
    public double ThreeDimensionalDepth(double nodeRadius)
    {
        return nodeRadius * _threeDimensionalDepthMultiplier;
    }

    /// <summary>
    /// The colour of the shape's border.
    /// </summary>
    public Color BorderColor { get; set; } = Color.Empty;

    /// <summary>
    /// The colour of the shape.
    /// </summary>
    public Color Color { get; set; } = Color.Empty;

    /// <summary>
    /// The color to use to start the gradient on the side of the shape in pseudo-3D.
    /// </summary>
    public Color GradientStartColor
    {
        get
        {
            if (_gradientStartColor == Color.Empty)
            {
                _gradientStartColor = Color.FromArgb(Color.A,
                                                     Color.R,
                                                     Color.G,
                                                     Color.B);
            }

            return _gradientStartColor;
        }
        set
        {
            _gradientStartColor = value;
        }
    }

    /// <summary>
    /// The color to use to end the gradient on the side of the shape in pseudo-3D.
    /// </summary>
    public Color GradientEndColor
    {
        get
        {
            if (_gradientEndColor == Color.Empty)
            {
                float factor = 0.8f;

                int r = (int)Math.Clamp(Color.R * factor, 0, 255);
                int g = (int)Math.Clamp(Color.G * factor, 0, 255);
                int b = (int)Math.Clamp(Color.B * factor, 0, 255);

                _gradientEndColor = Color.FromArgb(Color.A, r, g, b);
            }

            return _gradientEndColor;
        }
    }

    /// <summary>
    /// The color to use to start the gradient on the side of the shape in pseudo-3D.
    /// </summary>
    public Color BorderGradientStartColor
    {
        get
        {
            if (_borderGradientStartColor == Color.Empty)
            {
                _borderGradientStartColor = Color.FromArgb(BorderColor.A,
                                                           BorderColor.R,
                                                           BorderColor.G,
                                                           BorderColor.B);
            }

            return _borderGradientStartColor;
        }
        set
        {
            _borderGradientStartColor = value;
        }
    }

    /// <summary>
    /// The color to use to end the gradient on the side of the shape in pseudo-3D.
    /// </summary>
    public Color BorderGradientEndColor
    {
        get
        {
            if (_borderGradientEndColor == Color.Empty)
            {
                float factor = 0.8f;

                int r = (int)Math.Clamp(BorderColor.R * factor, 0, 255);
                int g = (int)Math.Clamp(BorderColor.G * factor, 0, 255);
                int b = (int)Math.Clamp(BorderColor.B * factor, 0, 255);

                _borderGradientEndColor = Color.FromArgb(Color.A, r, g, b);
            }

            return _borderGradientEndColor;
        }
    }

    /// <summary>
    /// Whether or not this shape is influenced by the light source.
    /// </summary>
    public bool HasLightSourceImpact { get; set; }

    /// <summary>
    /// The radius of the shape
    /// </summary>
    public double Radius { get; set; }

    /// <summary>
    /// Set the start and end of the gradient of the front and sides of the shape.
    /// </summary>
    /// <param name="frontFaceStartPoint"></param>
    /// <param name="frontFaceEndPoint"></param>
    /// <param name="sideFaceStartPoint"></param>
    /// <param name="sideFaceEndPoint"></param>
    public void SetNodeGradientPoints((double X, double Y) frontFaceStartPoint,
                                      (double X, double Y) frontFaceEndPoint,
                                      (double X, double Y) sideFaceStartPoint,
                                      (double X, double Y) sideFaceEndPoint)
    {
        FrontFaceGradientStartPoint = frontFaceStartPoint;
        FrontFaceGradientEndPoint = frontFaceEndPoint;
        SideFaceGradientStartPoint = sideFaceStartPoint;
        SideFaceGradientEndPoint = sideFaceEndPoint;
    }
}