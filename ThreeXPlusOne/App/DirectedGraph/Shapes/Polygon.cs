using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Polygon() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Polygon;

    /// <summary>
    /// Draw a regular polygon, from sides 3-8, including squares but not including other 4-sided shapes
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="numberOfSides"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureRegularPolygon((double X, double Y) nodePosition,
                                         double nodeRadius,
                                         int numberOfSides,
                                         double rotationAngle)
    {
        _shapeConfiguration.PolygonConfiguration = new();

        for (int i = 0; i < numberOfSides; i++)
        {
            double angle = (2 * Math.PI / numberOfSides * i) + rotationAngle;

            (double X, double Y) vertex = (nodePosition.X + nodeRadius * Math.Cos(angle),
                                           nodePosition.Y + nodeRadius * Math.Sin(angle));

            _shapeConfiguration.PolygonConfiguration.Vertices.Add(vertex);
        }
    }

    /// <summary>
    /// Configure the points of a rectangle
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureRectangle((double X, double Y) nodePosition,
                                    double nodeRadius,
                                    double rotationAngle)
    {
        _shapeConfiguration.PolygonConfiguration = new();

        double width = nodeRadius;
        double height = width / 2;

        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - width / 2, nodePosition.Y - height / 2), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + width / 2, nodePosition.Y - height / 2), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + width / 2, nodePosition.Y + height / 2), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - width / 2, nodePosition.Y + height / 2), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure the points of a rhombus
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureRhombus((double X, double Y) nodePosition,
                                  double nodeRadius,
                                  double rotationAngle)
    {
        _shapeConfiguration.PolygonConfiguration = new();

        double angleRadians = Math.PI * 60 / 180;
        double halfDiagonal = nodeRadius * Math.Cos(angleRadians / 2);

        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y - halfDiagonal), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + halfDiagonal, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y + halfDiagonal), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - halfDiagonal, nodePosition.Y), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure the points of a parallelogram
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureParallelogram((double X, double Y) nodePosition,
                                   double nodeRadius,
                                   double rotationAngle)
    {
        _shapeConfiguration.PolygonConfiguration = new();

        double baseLength = nodeRadius / 2;
        double sideLength = nodeRadius;
        double angleRadians = Math.PI * 30 / 180;
        double height = sideLength * Math.Sin(angleRadians);

        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + baseLength, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + baseLength - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        int numberOfSides = Random.Shared.Next(3, 9); //min: triangle, max: octagon
        double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

        if (numberOfSides != 4)
        {
            ConfigureRegularPolygon(nodePosition, nodeRadius, numberOfSides, rotationAngle);

            return;
        }

        int fourSidedShapeChoice = Random.Shared.Next(1, 5);

        switch (fourSidedShapeChoice)
        {
            case 1: // Square
                ConfigureRegularPolygon(nodePosition, nodeRadius, 4, rotationAngle);
                break;

            case 2:
                ConfigureRectangle(nodePosition, nodeRadius, rotationAngle);
                break;

            case 3:
                ConfigureRhombus(nodePosition, nodeRadius, rotationAngle);
                break;

            case 4:
                ConfigureParallelogram(nodePosition, nodeRadius, rotationAngle);
                break;
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