using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Polygon() : Shape, IShape
{
    private readonly int _numberOfSides = Random.Shared.Next(3, 9); //min: triangle, max: octagon

    public ShapeType ShapeType => ShapeType.Polygon;

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        _shapeConfiguration.PolygonConfiguration = new();

        double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

        for (int i = 0; i < _numberOfSides; i++)
        {
            double angle = (2 * Math.PI / _numberOfSides * i) + rotationAngle;

            (double X, double Y) vertex = (nodePosition.X + nodeRadius * Math.Cos(angle),
                                           nodePosition.Y + nodeRadius * Math.Sin(angle));

            _shapeConfiguration.PolygonConfiguration.Vertices.Add(vertex);
        }
    }

    /// <summary>
    /// Apply skew settings to the shape
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeSkew((double X, double Y) nodePosition,
                             double nodeRadius)
    {
        GenerateShapeSkew();
    }
}