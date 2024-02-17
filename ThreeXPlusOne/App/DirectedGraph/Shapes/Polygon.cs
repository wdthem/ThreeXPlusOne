using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Polygon() : Shape, IShape
{
    private readonly int _numberOfSides = Random.Shared.Next(3, 9); //min: triangle, max: octagon

    public ShapeType ShapeType => ShapeType.Polygon;

    /// <summary>
    /// Apply a skew to a vertex of a polygon to give a pseudo-3D effect to the node shape
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="center"></param>
    /// <param name="skewFactor"></param>
    /// <param name="rotationRadians"></param>
    /// <returns></returns>
    private static (double X, double Y) ApplyVertexSkew((double X, double Y) vertex,
                                                        (double X, double Y) center,
                                                        double skewFactor,
                                                        double rotationRadians)
    {
        double dx = vertex.X - center.X;
        double dy = vertex.Y - center.Y;

        double rotatedX = dx * Math.Cos(rotationRadians) - dy * Math.Sin(rotationRadians);
        double rotatedY = dx * Math.Sin(rotationRadians) + dy * Math.Cos(rotationRadians);

        double skewedX = rotatedX + skewFactor * rotatedY;
        double skewedY = rotatedY;

        return (center.X + skewedX, center.Y + skewedY);
    }

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
        if (_shapeConfiguration.PolygonVertices.Count == 0)
        {
            double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

            for (int i = 0; i < _numberOfSides; i++)
            {
                double angle = (2 * Math.PI / _numberOfSides * i) + rotationAngle;

                (double X, double Y) polygonVertex = (nodePosition.X + nodeRadius * Math.Cos(angle),
                                                      nodePosition.Y + nodeRadius * Math.Sin(angle));

                _shapeConfiguration.PolygonVertices.Add(polygonVertex);
            }

            return;
        }

        //apply skew
        if (skewFactor != null &&
            skewFactor.Value > 0 &&
            _shapeConfiguration.PolygonVertices.Count > 0)
        {
            double rotationRadians = -0.785 + Random.Shared.NextDouble() * 1.57; // Range of -π/4 to π/2 radians

            for (int lcv = 0; lcv < _shapeConfiguration.PolygonVertices.Count; lcv++)
            {
                _shapeConfiguration.PolygonVertices[lcv] =
                    ApplyVertexSkew(_shapeConfiguration.PolygonVertices[lcv], nodePosition, skewFactor.Value, rotationRadians);
            }
        }
    }
}