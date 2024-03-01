using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Polygon() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Polygon;

    //This class can generate 10 different polygons, thus the weighting of 10 to 
    //put each polygon type on equal footing to all other distinct shapes
    public int SelectionWeight => 10;

    public bool HasGap => false;

    /// <summary>
    /// Get a randomly-selected number of sides from 3 to 8 (min: triangle, max: octagon)
    /// </summary>
    /// <remarks>
    /// More weight is applied toward selecting a four-sided shape given there are multiple 4-sided shapes
    /// </remarks>
    /// <returns></returns>
    private static int GenerateNumberOfSides()
    {
        int[] weights = [1, 6, 1, 1, 1, 1];  // Weights for numbers 3, 4, 5, 6, 7, 8

        List<KeyValuePair<ShapeType, ShapeSelectionWeight>> shapeWeightsList = ShapeHelper.ConfigureShapeSelectionWeights(weights);

        int totalWeight = shapeWeightsList.Sum(pair => pair.Value.Weight);
        int randomNumber = Random.Shared.Next(1, totalWeight + 1);

        int numberOfSides = Enumerable.Range(0, shapeWeightsList.Count)
                                      .FirstOrDefault(i => randomNumber <= shapeWeightsList[i].Value.CumulativeWeight) + 3; // Add 3 to the index to get the correct number in the range

        return numberOfSides;
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
        _shapeConfiguration.Vertices = [];

        for (int i = 0; i < numberOfSides; i++)
        {
            double angle = (2 * Math.PI / numberOfSides * i) + rotationAngle;

            (double X, double Y) vertex = (nodePosition.X + nodeRadius * Math.Cos(angle),
                                           nodePosition.Y + nodeRadius * Math.Sin(angle));

            _shapeConfiguration.Vertices.Add(vertex);
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
        _shapeConfiguration.Vertices = [];

        double width = nodeRadius * 2;
        double height = nodeRadius;

        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - width / 2, nodePosition.Y - height / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + width / 2, nodePosition.Y - height / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + width / 2, nodePosition.Y + height / 2), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - width / 2, nodePosition.Y + height / 2), nodePosition, rotationAngle));
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
        _shapeConfiguration.Vertices = [];

        double horizontalDistance = nodeRadius * 1.5 * Math.Cos(Math.PI / 6); // 30 degrees
        double verticalDistance = nodeRadius * 1.5 * Math.Sin(Math.PI / 6); // 30 degrees

        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y - verticalDistance), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + horizontalDistance, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y + verticalDistance), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - horizontalDistance, nodePosition.Y), nodePosition, rotationAngle));
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
        _shapeConfiguration.Vertices = [];

        double baseLength = nodeRadius * 2;
        double sideLength = nodeRadius * 1.75;
        double angleRadians = Math.PI * 30 / 180;
        double height = sideLength * Math.Sin(angleRadians);

        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + baseLength, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + baseLength - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
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
        _shapeConfiguration.Vertices = [];

        double topWidth = nodeRadius * 2 * 0.667;
        double height = nodeRadius * 1.4;

        double halfTopWidth = topWidth / 2;
        double halfBottomWidth = topWidth;

        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - halfTopWidth, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + halfTopWidth, nodePosition.Y), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X - halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure the points of a kite
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureKite((double X, double Y) nodePosition,
                               double nodeRadius,
                               double rotationAngle)
    {
        _shapeConfiguration.Vertices = [];

        double angleLeft = Math.PI / 4;         // 45° in radians
        double angleRight = 3 * Math.PI / 4;    // 135° in radians

        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y - nodeRadius), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + nodeRadius * Math.Cos(angleRight), nodePosition.Y + nodeRadius * Math.Sin(angleRight)), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y + nodeRadius), nodePosition, rotationAngle));
        _shapeConfiguration.Vertices.Add(RotateVertex((nodePosition.X + nodeRadius * Math.Cos(angleLeft), nodePosition.Y + nodeRadius * Math.Sin(angleLeft)), nodePosition, rotationAngle));
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

        int fourSidedShapeChoice = Random.Shared.Next(1, 7);

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

            case 6:
                ConfigureKite(nodePosition, nodeRadius, rotationAngle);
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