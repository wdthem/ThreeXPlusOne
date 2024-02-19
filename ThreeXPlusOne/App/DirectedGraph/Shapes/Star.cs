using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Star() : Shape, IShape
{
    private readonly int _numberOfPoints = 5;

    public ShapeType ShapeType => ShapeType.Star;

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
        //adding skew
        if (_shapeConfiguration.StarConfiguration != null)
        {
            _shapeConfiguration.Skew = GetShapeSkew(skewFactor);

            return;
        }

        _shapeConfiguration.StarConfiguration = new()
        {
            InnerRadius = nodeRadius * 0.4,
            AngleIncrement = 2 * Math.PI / (_numberOfPoints * 2)
        };

        for (int i = 0; i < _numberOfPoints * 2; i++)
        {
            double angle = i * _shapeConfiguration.StarConfiguration.AngleIncrement;
            double radius = (i % 2 == 0) 
                                    ? nodeRadius 
                                    : _shapeConfiguration.StarConfiguration.InnerRadius;

            double x = nodePosition.X + (radius * Math.Cos(angle));
            double y = nodePosition.Y + (radius * Math.Sin(angle));

            _shapeConfiguration.StarConfiguration.AngleVertices.Add((x, y));
        }
    }
}
