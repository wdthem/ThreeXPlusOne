using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.NodeShapes;

public class Arc() : Shape, IShape
{
    private readonly int _sweepAngle = 180;

    public ShapeType ShapeType => ShapeType.Arc;

    public int SelectionWeight => 1;

    public bool HasGap => true;

    /// <summary>
    /// The angle in degrees at which the arc begins. Measured clockwise from the positive x-axis (3 o'clock position). 
    /// </summary>
    public double TopArcStartAngle { get; set; }

    /// <summary>
    /// The angle in degrees at which the arc begins. Measured clockwise from the positive x-axis (3 o'clock position). 
    /// </summary>
    public double BottomArcStartAngle { get; set; }

    /// <summary>
    /// The angle in degrees that the arc covers. Positive values indicate a clockwise sweep, while negative values indicate a counterclockwise sweep. 
    /// </summary>
    public double TopArcSweepAngle { get; set; }

    /// <summary>
    /// The angle in degrees that the arc covers. Positive values indicate a clockwise sweep, while negative values indicate a counterclockwise sweep. 
    /// </summary>
    public double BottomArcSweepAngle { get; set; }

    /// <summary>
    /// The bounding box used to render the top arc shape.
    /// </summary>
    public ShapeBounds TopArcBounds { get; set; } = new();

    /// <summary>
    /// The bounding box used to render the top arc shape.
    /// </summary>
    public ShapeBounds BottomArcBounds { get; set; } = new();

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        double thickness = Random.Shared.Next((int)nodeRadius / 2, (int)nodeRadius);

        float innerRadius = (float)nodeRadius - (float)thickness / 2;
        float outerRadius = (float)nodeRadius + (float)thickness / 2;
        int startAngle = Random.Shared.Next(360);

        TopArcStartAngle = startAngle;
        BottomArcStartAngle = startAngle + _sweepAngle;
        TopArcSweepAngle = _sweepAngle;
        BottomArcSweepAngle = -_sweepAngle;

        TopArcBounds = new ShapeBounds
        {
            Left = nodePosition.X - outerRadius,
            Top = nodePosition.Y - outerRadius,
            Right = nodePosition.X + outerRadius,
            Bottom = nodePosition.Y + outerRadius
        };

        BottomArcBounds = new ShapeBounds
        {
            Left = nodePosition.X - innerRadius,
            Top = nodePosition.Y - innerRadius,
            Right = nodePosition.X + innerRadius,
            Bottom = nodePosition.Y + innerRadius
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
        GenerateShapeSkew();
    }
}