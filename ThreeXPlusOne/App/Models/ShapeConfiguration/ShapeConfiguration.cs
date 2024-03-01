using System.Drawing;

namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record ShapeConfiguration()
{
    private readonly double _threeDimensionalDepthMultiplier = 0.1;

    private readonly int _threeDimensionalSideCount = 360;

    /// <summary>
    /// The vertices of the given shape
    /// </summary>
    public List<(double X, double Y)>? Vertices { get; set; }

    /// <summary>
    /// The coordinates and radii of an ellipse
    /// </summary>
    public EllipseConfiguration? EllipseConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing an arc shape
    /// </summary>
    public ArcConfiguration? ArcConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing a pill shape
    /// </summary>
    public PillConfiguration? PillConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing a semicircle shape
    /// </summary>
    public SemiCircleConfiguration? SemiCircleConfiguration { get; set; }

    /// <summary>
    /// Skew values applied to the shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y)? Skew { get; set; }

    /// <summary>
    /// The depth of the shape when rendered in pseudo-3D, based on the node radius
    /// </summary>
    /// <param name="nodeRadius"></param>
    /// <returns></returns>
    public double ThreeDimensionalDepth(double nodeRadius)
    {
        return nodeRadius * _threeDimensionalDepthMultiplier;
    }

    /// <summary>
    /// The number of sides to render when drawing the shape in pseudo-3D
    /// </summary>
    public int ThreeDimensionalSideCount => _threeDimensionalSideCount;

    /// <summary>
    /// The start point of the gradient for the shape's side
    /// </summary>
    public (double X, double Y) ThreeDimensionalSideGradientStartPoint { get; set; }

    /// <summary>
    /// The end point of the gradient for the shape's side
    /// </summary>
    public (double X, double Y) ThreeDimensionalSideGradientEndPoint { get; set; }

    /// <summary>
    /// The start point of the gradient for the shape's front face
    /// </summary>
    public (double X, double Y) ThreeDimensionalFrontFaceGradientStartPoint { get; set; }

    /// <summary>
    /// The end point of the gradient for the shape's front face
    /// </summary>
    public (double X, double Y) ThreeDimensionalFrontFaceGradientEndPoint { get; set; }
}