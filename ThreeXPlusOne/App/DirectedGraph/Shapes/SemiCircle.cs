using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class SemiCircle() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.SemiCircle;

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
        if (_shapeConfiguration.SemiCircleConfiguration != null)
        {
            _shapeConfiguration.SemiCircleConfiguration.Skew = GetShapeSkew(skewFactor);

            return;
        }

        _shapeConfiguration.SemiCircleConfiguration = new()
        {
            Orientation = Random.Shared.Next(0, 360),
            ShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - nodeRadius,
                Top = nodePosition.Y - nodeRadius,
                Right = nodePosition.X + nodeRadius,
                Bottom = nodePosition.Y + nodeRadius
            }
        };
    }
}