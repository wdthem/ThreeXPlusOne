using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Pill() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Pill;

    public int SelectionWeight => 1;

    public bool HasGap => false;

    /// <summary>
    /// The height of the pill shape.
    /// </summary>
    /// <remarks>
    /// Width is determined by the node radius.
    /// </remarks>
    public double Height { get; set; }

    /// <summary>
    /// The angle by which the shape is rotated on the graph.
    /// </summary>
    public double RotationAngle { get; set; }

    /// <summary>
    /// The x-radius of the curve of the pill shape.
    /// </summary>
    public double CurveRadiusX { get; set; }

    /// <summary>
    /// The y-radius of the curve of the pill shape.
    /// </summary>
    public double CurveRadiusY { get; set; }

    /// <summary>
    /// The bounding box used to render the pill shape.
    /// </summary>
    public ShapeBounds Bounds { get; set; } = new();

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        double pillWidth = nodeRadius;
        double pillHeight = nodeRadius * 2;

        Height = pillHeight;
        RotationAngle = Random.Shared.Next(360);

        Bounds = new ShapeBounds
        {
            Left = nodePosition.X - pillWidth / 2,
            Top = nodePosition.Y - pillHeight / 2,
            Right = nodePosition.X + pillWidth / 2,
            Bottom = nodePosition.Y + pillHeight / 2
        };

        CurveRadiusX = pillHeight / 2;
        CurveRadiusY = pillHeight / 2;
    }

    /// <summary>
    /// Apply skew settings to the shape.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeSkew((double X, double Y) nodePosition,
                             double nodeRadius)
    {
        GenerateShapeSkew();
    }
}