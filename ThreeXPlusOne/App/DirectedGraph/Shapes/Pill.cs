using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Pill() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Pill;

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="skewFactor"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius,
                                      double? skewFactor = null)
    {
        //just adding skew
        if (_shapeConfiguration.PillConfiguration != null)
        {
            _shapeConfiguration.PillConfiguration.Skew = GetShapeSkew(skewFactor);

            return;
        }

        double pillWidth = nodeRadius;
        double pillHeight = nodeRadius * 2;

        _shapeConfiguration.PillConfiguration = new()
        {
            Height = pillHeight,
            RotationAngle = Random.Shared.Next(0, 360),
            PillBounds = new ShapeBounds
            {
                Left = nodePosition.X - pillWidth / 2,
                Top = nodePosition.Y - pillHeight / 2,
                Right = nodePosition.X + pillWidth / 2,
                Bottom = nodePosition.Y + pillHeight / 2
            },
            PillCurveRadiusX = pillHeight / 2,
            PillCurveRadiusY = pillHeight / 2
        };
    }
}