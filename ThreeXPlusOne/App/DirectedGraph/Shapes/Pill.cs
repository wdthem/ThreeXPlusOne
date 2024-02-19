using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Pill() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Pill;

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        double pillWidth = nodeRadius;
        double pillHeight = nodeRadius * 2;

        _shapeConfiguration.PillConfiguration = new()
        {
            Height = pillHeight,
            RotationAngle = Random.Shared.Next(360),
            ShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - pillWidth / 2,
                Top = nodePosition.Y - pillHeight / 2,
                Right = nodePosition.X + pillWidth / 2,
                Bottom = nodePosition.Y + pillHeight / 2
            },
            CurveRadiusX = pillHeight / 2,
            CurveRadiusY = pillHeight / 2
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