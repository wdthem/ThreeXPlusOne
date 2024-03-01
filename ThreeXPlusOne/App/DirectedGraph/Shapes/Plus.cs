using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Plus() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Plus;

    public int SelectionWeight => 1;

    public bool HasGap => false;

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        double size = nodeRadius * 2;
        double thickness = nodeRadius / 2.5;
        double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

        _shapeConfiguration.Vertices = [];

        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - thickness / 2, nodePosition.Y - size / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + thickness / 2, nodePosition.Y - size / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + thickness / 2, nodePosition.Y - thickness / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + size / 2, nodePosition.Y - thickness / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + size / 2, nodePosition.Y + thickness / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + thickness / 2, nodePosition.Y + thickness / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + thickness / 2, nodePosition.Y + size / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - thickness / 2, nodePosition.Y + size / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - thickness / 2, nodePosition.Y + thickness / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - size / 2, nodePosition.Y + thickness / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - size / 2, nodePosition.Y - thickness / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - thickness / 2, nodePosition.Y - thickness / 2), nodePosition, rotationAngle));
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
