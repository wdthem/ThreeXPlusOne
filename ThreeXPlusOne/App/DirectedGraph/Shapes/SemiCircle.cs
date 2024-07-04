using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class SemiCircle() : Shape, IShape
{
    private readonly double _sweepAngle = 180;

    public ShapeType ShapeType => ShapeType.SemiCircle;

    public int SelectionWeight => 1;

    public bool HasGap => false;

    /// <summary>
    /// Specifies the angle direction in which the flat side of the semicircle is facing.
    /// </summary>
    public double Orientation { get; set; }

    /// <summary>
    /// The angle in degrees that the semicircle covers. 
    /// </summary>
    /// <remarks>
    /// Constant value of 180 for a semicircle
    /// </remarks>
    public double SweepAngle => _sweepAngle;

    /// <summary>
    /// The bounding box used to render the semicircle shape
    /// </summary>
    public ShapeBounds ShapeBounds { get; set; } = new();

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        Orientation = Random.Shared.Next(360);

        ShapeBounds = new ShapeBounds
        {
            Left = nodePosition.X - nodeRadius,
            Top = nodePosition.Y - nodeRadius,
            Right = nodePosition.X + nodeRadius,
            Bottom = nodePosition.Y + nodeRadius
        };
    }

    /// <summary>
    /// Apply skew settings to the shape
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeSkew((double X, double Y) nodePosition,
                             double nodeRadius)
    {
        GenerateShapeSkew();
    }
}