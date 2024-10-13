using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.NodeShapes;

public class Polygon() : Shape, IVertexShape
{
    /// <summary>
    /// Selection weighting for each possible number of polygon sides.
    /// </summary>
    /// <remarks>
    /// There are multiple possible 4-sided polygons, thus more weight is applied.
    /// </remarks>
    private readonly Dictionary<int, int> _polygonSideWeights = new()
    {
        {3, 1},
        {4, 6},
        {5, 1},
        {6, 1},
        {7, 1},
        {8, 1},
    };

    public ShapeType ShapeType => ShapeType.Polygon;

    /// <summary>
    /// This class can generate multiple types of polygons, therefore the selection weight
    /// must match the total number of shapes possible.
    /// </summary>
    public int SelectionWeight => _polygonSideWeights.Values.Sum();

    public bool HasGap => false;

    public List<(double X, double Y)> Vertices { get; set; } = [];

    /// <summary>
    /// Get a randomly-selected (and weighted) number of sides from 3 to 8 (min: triangle, max: octagon).
    /// </summary>
    /// <returns></returns>
    private int GetNumberOfSides()
    {
        List<KeyValuePair<ShapeType, ShapeSelectionWeight>> shapeWeightsList =
            ShapeSelectionWeightProvider.ConfigureShapeSelectionWeights(_polygonSideWeights);

        int totalWeight = shapeWeightsList.Sum(pair => pair.Value.Weight);
        int randomWeight = Random.Shared.Next(1, totalWeight + 1); //only adding 1 here because the max value of Random.Shared.Next is exclusive 

        //find the first index number in the shapeWeightsList where the random weight value 
        //is less than or equal to the cumulative weight value stored in that array element (this yields: 0, 1, 2, 3 etc.)
        //then add 3 to that value to correctly set the number of sides of the corresponding polygon
        return Enumerable.Range(0, shapeWeightsList.Count)
                         .FirstOrDefault(i => randomWeight <= shapeWeightsList[i].Value.CumulativeWeight) + 3;
    }

    /// <summary>
    /// Configure a regular polygon, from sides 3-8, including squares but not including other 4-sided polygons.
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
        for (int i = 0; i < numberOfSides; i++)
        {
            double angle = (2 * Math.PI / numberOfSides * i) + rotationAngle;

            (double X, double Y) vertex = (nodePosition.X + nodeRadius * Math.Cos(angle),
                                           nodePosition.Y + nodeRadius * Math.Sin(angle));

            Vertices.Add(vertex);
        }
    }

    /// <summary>
    /// Configure a rectangle.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureRectangle((double X, double Y) nodePosition,
                                    double nodeRadius,
                                    double rotationAngle)
    {
        double width = nodeRadius * 2;
        double height = nodeRadius;

        Vertices.Add(RotateVertex((nodePosition.X - width / 2, nodePosition.Y - height / 2), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + width / 2, nodePosition.Y - height / 2), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + width / 2, nodePosition.Y + height / 2), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X - width / 2, nodePosition.Y + height / 2), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure a rhombus.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureRhombus((double X, double Y) nodePosition,
                                  double nodeRadius,
                                  double rotationAngle)
    {
        double horizontalDistance = nodeRadius * 1.5 * Math.Cos(Math.PI / 6); // 30 degrees
        double verticalDistance = nodeRadius * 1.5 * Math.Sin(Math.PI / 6); // 30 degrees

        Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y - verticalDistance), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + horizontalDistance, nodePosition.Y), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y + verticalDistance), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X - horizontalDistance, nodePosition.Y), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure a parallelogram.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureParallelogram((double X, double Y) nodePosition,
                                        double nodeRadius,
                                        double rotationAngle)
    {
        double baseLength = nodeRadius * 2;
        double sideLength = nodeRadius * 1.75;
        double angleRadians = Math.PI * 30 / 180;
        double height = sideLength * Math.Sin(angleRadians);

        Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + baseLength, nodePosition.Y), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + baseLength - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X - sideLength * Math.Cos(angleRadians), nodePosition.Y + height), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure a trapezoid.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureTrapezoid((double X, double Y) nodePosition,
                                    double nodeRadius,
                                    double rotationAngle)
    {
        double topWidth = nodeRadius * 2 * 0.667;
        double height = nodeRadius * 1.4;

        double halfTopWidth = topWidth / 2;
        double halfBottomWidth = topWidth;

        Vertices.Add(RotateVertex((nodePosition.X - halfTopWidth, nodePosition.Y), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + halfTopWidth, nodePosition.Y), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X - halfBottomWidth, nodePosition.Y + height), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Configure a kite.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="rotationAngle"></param>
    private void ConfigureKite((double X, double Y) nodePosition,
                               double nodeRadius,
                               double rotationAngle)
    {
        double angleLeft = Math.PI / 4;         // 45° in radians
        double angleRight = 3 * Math.PI / 4;    // 135° in radians

        Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y - nodeRadius), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + nodeRadius * Math.Cos(angleRight), nodePosition.Y + nodeRadius * Math.Sin(angleRight)), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X, nodePosition.Y + nodeRadius), nodePosition, rotationAngle));
        Vertices.Add(RotateVertex((nodePosition.X + nodeRadius * Math.Cos(angleLeft), nodePosition.Y + nodeRadius * Math.Sin(angleLeft)), nodePosition, rotationAngle));
    }

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        int numberOfSides = GetNumberOfSides();
        double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

        if (numberOfSides != 4)
        {
            ConfigureRegularPolygon(nodePosition, nodeRadius, numberOfSides, rotationAngle);

            return;
        }

        int fourSidedShapeChoice = Random.Shared.Next(1, _polygonSideWeights[4] + 1);

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

            default:
                throw new ApplicationException($"No four-sided polygon configuration method for selection weight {fourSidedShapeChoice}");
        }
    }

    /// <summary>
    /// Apply skew settings to the shape.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeSkew((double X, double Y) nodePosition,
                             double nodeRadius)
    {
        GenerateShapeSkew();
    }
}