using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Arc() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Arc;

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
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
        int sweepAngle = 180;

        _shapeConfiguration.ArcConfiguration = new()
        {
            TopArcStartAngle = startAngle,
            BottomArcStartAngle = startAngle + sweepAngle,
            TopArcSweepAngle = sweepAngle,
            BottomArcSweepAngle = -sweepAngle,
            TopArcBounds = new ShapeBounds
            {
                Left = nodePosition.X - outerRadius,
                Top = nodePosition.Y - outerRadius,
                Right = nodePosition.X + outerRadius,
                Bottom = nodePosition.Y + outerRadius
            },
            BottomArcBounds = new ShapeBounds
            {
                Left = nodePosition.X - innerRadius,
                Top = nodePosition.Y - innerRadius,
                Right = nodePosition.X + innerRadius,
                Bottom = nodePosition.Y + innerRadius
            }
        };
    }

    /// <summary>
    /// Apply skew settings to the shape
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void GenerateShapeSkew((double X, double Y) nodePosition,
                                  double nodeRadius)
    {
        SetShapeSkew();
    }
}