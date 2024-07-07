using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

/// <summary>
/// For Elliptical shapes, which includes Circles because they are special-case ellipses.
/// </summary>
public class Ellipse() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Ellipse;

    public int SelectionWeight => 1;

    public bool HasGap => false;

    /// <summary>
    /// The x-radius of the ellipse.
    /// </summary>
    public double RadiusX { get; set; }

    /// <summary>
    /// The y-radius of the ellipse.
    /// </summary>
    public double RadiusY { get; set; }

    /// <summary>
    /// The bounding box used to render the ellipse shape.
    /// </summary>
    public ShapeBounds Bounds { get; set; } = new();

    /// <summary>
    /// Stretch the ellipse radii for skewed shapes.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    private void StretchRadii((double X, double Y) nodePosition,
                              double nodeRadius)
    {
        double skewFactor = (Random.Shared.NextDouble() > 0.5 ? 1 : -1) *
                            ((0.1 + Random.Shared.NextDouble()) * 0.6) *
                            0.6;  //reduce the overall impact to ellipses

        double horizontalOffset = nodeRadius * skewFactor;
        double verticalOffset = nodeRadius * (skewFactor * Random.Shared.NextDouble());

        RadiusX = nodeRadius + horizontalOffset;
        RadiusY = nodeRadius + verticalOffset;

        Bounds = new ShapeBounds
        {
            Left = nodePosition.X - RadiusX,
            Top = nodePosition.Y - RadiusY,
            Right = nodePosition.X + RadiusX,
            Bottom = nodePosition.Y + RadiusY
        };
    }

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        RadiusX = nodeRadius;
        RadiusY = nodeRadius;

        Bounds = new ShapeBounds
        {
            Left = nodePosition.X - nodeRadius,
            Top = nodePosition.Y - nodeRadius,
            Right = nodePosition.X + nodeRadius,
            Bottom = nodePosition.Y + nodeRadius
        };
    }

    /// <summary>
    /// Apply skew settings to the shape.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeSkew((double X, double Y) nodePosition,
                             double nodeRadius)
    {
        StretchRadii(nodePosition, nodeRadius);

        GenerateShapeSkew();
    }
}