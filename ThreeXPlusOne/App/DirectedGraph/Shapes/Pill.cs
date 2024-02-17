using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

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
        (double X, double Y) skew = (0, 0);

        //just adding skew
        if (_shapeConfiguration.PillConfiguration != null)
        {
            if (skewFactor != null && skewFactor > 0)
            {
                double skewX = skewFactor.Value;
                double skewY = skewX * Random.Shared.NextDouble();

                skew = (skewX, skewY);
            }

            _shapeConfiguration.PillConfiguration.Skew = skew;

            return;
        }

        _shapeConfiguration.PillConfiguration = new()
        {
            Height = nodeRadius * 2,
            RotationAngle = Random.Shared.Next(0, 360),
            Skew = skew
        };
    }
}