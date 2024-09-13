using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.NodeShapes;

public class Donut() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Donut;

    public int SelectionWeight => 1;

    public bool HasGap => false;

    /// <summary>
    /// The x-radius of the donut.
    /// </summary>
    public double RadiusX { get; set; }

    /// <summary>
    /// The y-radius of the donut.
    /// </summary>
    public double RadiusY { get; set; }

    /// <summary>
    /// The bounding box used to render the outside of the donut shape.
    /// </summary>
    public ShapeBounds OuterEllipseBounds { get; set; } = new();

    /// <summary>
    /// The bounding box used to render the inside of the donut shape.
    /// </summary>
    public ShapeBounds InnerEllipseBounds { get; set; } = new();

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
                            0.6;  //reduce the overall impact to ellipses (donuts are made of ellipses)

        double horizontalOffset = nodeRadius * skewFactor;
        double verticalOffset = nodeRadius * (skewFactor * Random.Shared.NextDouble());

        RadiusX = nodeRadius + horizontalOffset;
        RadiusY = nodeRadius + verticalOffset;

        OuterEllipseBounds = new ShapeBounds
        {
            Left = nodePosition.X - RadiusX,
            Top = nodePosition.Y - RadiusY,
            Right = nodePosition.X + RadiusX,
            Bottom = nodePosition.Y + RadiusY
        };

        InnerEllipseBounds = new ShapeBounds
        {
            Left = nodePosition.X - (RadiusX / 2),
            Top = nodePosition.Y - (RadiusY / 2),
            Right = nodePosition.X + (RadiusX / 2),
            Bottom = nodePosition.Y + (RadiusY / 2)
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

        OuterEllipseBounds = new ShapeBounds
        {
            Left = nodePosition.X - nodeRadius,
            Top = nodePosition.Y - nodeRadius,
            Right = nodePosition.X + nodeRadius,
            Bottom = nodePosition.Y + nodeRadius
        };

        InnerEllipseBounds = new ShapeBounds
        {
            Left = nodePosition.X - (nodeRadius / 2),
            Top = nodePosition.Y - (nodeRadius / 2),
            Right = nodePosition.X + (nodeRadius / 2),
            Bottom = nodePosition.Y + (nodeRadius / 2)
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