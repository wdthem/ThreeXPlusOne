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

        if (skewFactor != null && skewFactor > 0)
        {
            double skewX = skewFactor.Value;
            double skewY = skewX * Random.Shared.NextDouble();

            skew = (skewX, skewY);
        }

        //unset
        if (_shapeConfiguration.ArcConfig.StartAngle == 0)
        {
            double thickness = Random.Shared.Next((int)nodeRadius / 2, (int)nodeRadius);

            float innerRadius = (float)nodeRadius - (float)thickness / 2;
            float outerRadius = (float)nodeRadius + (float)thickness / 2;

            _shapeConfiguration.ArcConfig = (Random.Shared.Next(0, 360),
                                             180,
                                             innerRadius,
                                             outerRadius,
                                             skew);
        }

        //just adding skew
        else
        {
            _shapeConfiguration.ArcConfig = (_shapeConfiguration.ArcConfig.StartAngle,
                                             _shapeConfiguration.ArcConfig.SweepAngle,
                                             _shapeConfiguration.ArcConfig.InnerRadius,
                                             _shapeConfiguration.ArcConfig.OuterRadius,
                                             skew);
        }
    }
}