using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Arc() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Arc;

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
        if (_shapeConfiguration.ArcConfiguration != null)
        {
            if (skewFactor != null && skewFactor > 0)
            {
                double skewX = skewFactor.Value;
                double skewY = skewX * Random.Shared.NextDouble();

                skew = (skewX, skewY);
            }

            _shapeConfiguration.ArcConfiguration.Skew = skew;

            return;
        }

        double thickness = Random.Shared.Next((int)nodeRadius / 2, (int)nodeRadius);

        float innerRadius = (float)nodeRadius - (float)thickness / 2;
        float outerRadius = (float)nodeRadius + (float)thickness / 2;

        _shapeConfiguration.ArcConfiguration = new()
        {
            StartAngle = Random.Shared.Next(0, 360),
            SweepAngle = 180,
            InnerRadius = innerRadius,
            OuterRadius = outerRadius,
            Skew = skew
        };
    }
}