using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Trapezoid() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Trapezoid;

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

        double topWidth = nodeRadius * 0.667;
        double height = nodeRadius / 2;

        double halfTopWidth = topWidth / 2;
        double halfBottomWidth = nodeRadius / 2;

        _shapeConfiguration.TrapezoidConfiguration = new()
        {
            TopWidth = topWidth,

            TopLeftVertex = RotateVertex((nodePosition.X - halfTopWidth, nodePosition.Y), nodePosition, rotationAngle),
            TopRightVertex = RotateVertex((nodePosition.X + halfTopWidth, nodePosition.Y), nodePosition, rotationAngle),
            BottomRightVertex = RotateVertex((nodePosition.X + halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle),
            BottomLeftVertex = RotateVertex((nodePosition.X - halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle)
        };
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