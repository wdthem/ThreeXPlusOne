using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

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
        (double X, double Y) skew = (0, 0);

        //just adding skew
        if (_shapeConfiguration.SemiCircleConfiguration != null)
        {
            if (skewFactor != null && skewFactor > 0)
            {
                double skewX = skewFactor.Value;
                double skewY = skewX * Random.Shared.NextDouble();

                skew = (skewX, skewY);
            }

            _shapeConfiguration.SemiCircleConfiguration.Skew = skew;

            return;
        }

        _shapeConfiguration.SemiCircleConfiguration = new()
        {
            Orientation = Random.Shared.Next(0, 360),
            Skew = skew
        };
    }
}