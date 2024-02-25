using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Polygon() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Polygon;

    //This class can generate 10 different polygons, thus the weighting of 10 to 
    //put each polygon type on equal footing to all other distinct shapes
    public int SelectionWeight => 10;

    /// <summary>
    /// Get a randomly-selected number of sides from 3 to 8 (min: triangle, max: octagon)
    /// </summary>
    /// <remarks>
    /// More weight is applied toward selecting a four-sided shape given there are multiple 4-sided shapes
    /// </remarks>
    /// <returns></returns>
    private static int GenerateNumberOfSides()
    {
        int[] weights = [1, 5, 1, 1, 1, 1];  // Weights for numbers 3, 4, 5, 6, 7, 8

        int[] cumulativeWeights = new int[weights.Length];
        int totalWeight = 0;

        for (int i = 0; i < weights.Length; i++)
        {
            totalWeight += weights[i];
            cumulativeWeights[i] = totalWeight;
        }

        int randomNumber = Random.Shared.Next(1, totalWeight + 1);

        int numberOfSides = Enumerable.Range(0, cumulativeWeights.Length)
                                      .FirstOrDefault(i => randomNumber <= cumulativeWeights[i]) + 3; // Add 3 to the index to get the correct number in the range

        return numberOfSides;
    }

    /// <summary>
    /// Rotate the points so the shape is not always drawn in the same orientation
    /// </summary>
    /// <param name="point"></param>
    /// <param name="center"></param>
    /// <param name="angle"></param>
    /// <returns></returns>
    protected static (double X, double Y) RotateVertex((double X, double Y) point,
                                                       (double X, double Y) center,
                                                       double angle)
    {
        double cosAngle = Math.Cos(angle);
        double sinAngle = Math.Sin(angle);

        double x = cosAngle * (point.X - center.X) - sinAngle * (point.Y - center.Y) + center.X;
        double y = sinAngle * (point.X - center.X) + cosAngle * (point.Y - center.Y) + center.Y;

        return (x, y);
    }

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

        double width = nodeRadius * 2;
        double height = nodeRadius;

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

        double horizontalDistance = nodeRadius * 1.5 * Math.Cos(Math.PI / 6); // 30 degrees
        double verticalDistance = nodeRadius * 1.5 * Math.Sin(Math.PI / 6); // 30 degrees

        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y - verticalDistance), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + horizontalDistance, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y + verticalDistance), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - horizontalDistance, nodePosition.Y), nodePosition, rotationAngle));
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

        double baseLength = nodeRadius * 2;
        double sideLength = nodeRadius * 1.75;
        double angleRadians = Math.PI * 30 / 180;
        double height = sideLength * Math.Sin(angleRadians);

        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + baseLength, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + baseLength - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure the points of a trapezoid
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureTrapezoid((double X, double Y) nodePosition,
                                    double nodeRadius,
                                    double rotationAngle)
    {
        _shapeConfiguration.PolygonConfiguration = new();

        double topWidth = nodeRadius * 2 * 0.667;
        double height = nodeRadius * 1.4;

        double halfTopWidth = topWidth / 2;
        double halfBottomWidth = topWidth;

        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - halfTopWidth, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + halfTopWidth, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X + halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle));
        _shapeConfiguration.PolygonConfiguration.Vertices.Add(RotateVertex((nodePosition.X - halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        int numberOfSides = GenerateNumberOfSides();
        double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

        if (numberOfSides != 4)
        {
            ConfigureRegularPolygon(nodePosition, nodeRadius, numberOfSides, rotationAngle);

            return;
        }

        int fourSidedShapeChoice = Random.Shared.Next(1, 6);

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

            case 5:
                ConfigureTrapezoid(nodePosition, nodeRadius, rotationAngle);
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