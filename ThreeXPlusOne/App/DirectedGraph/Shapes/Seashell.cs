using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Seashell() : Shape, IShape
{
    private readonly int _spiralTurns = 3;
    private readonly double _angleStep = Math.PI / 20;

    public ShapeType ShapeType => ShapeType.Seashell;

    public int SelectionWeight => 1;

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

        _shapeConfiguration.SeashellConfiguration = new();

        // the spiral part of the seashell
        for (double angle = 0; angle < _spiralTurns * 2 * Math.PI; angle += _angleStep)
        {
            double radius = nodeRadius * angle / (_spiralTurns * 2 * Math.PI);
            double x = nodePosition.X + radius * Math.Cos(angle);
            double y = nodePosition.Y + radius * Math.Sin(angle);

            _shapeConfiguration.SeashellConfiguration.SpiralCoordinates.Add(RotateVertex((x, y), nodePosition, rotationAngle));
        }

        // the outer edge of the seashell
        for (double angle = _spiralTurns * 2 * Math.PI; angle >= 0; angle -= _angleStep)
        {
            double radius = nodeRadius * angle / (_spiralTurns * 2 * Math.PI) + nodeRadius / 4;
            double x = nodePosition.X + radius * Math.Cos(angle);
            double y = nodePosition.Y + radius * Math.Sin(angle);

            _shapeConfiguration.SeashellConfiguration.SpiralCoordinates.Add(RotateVertex((x, y), nodePosition, rotationAngle));
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