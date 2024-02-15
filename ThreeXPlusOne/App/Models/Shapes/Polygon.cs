using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces;

namespace ThreeXPlusOne.App.Models.Shapes;

public class Polygon() : Shape, IShape
{
    private readonly int _numberOfSides = Random.Shared.Next(3, 11);
    private readonly ShapeConfiguration _shapeConfiguration = new();

    public ShapeType ShapeType => ShapeType.Polygon;

    /// <summary>
    /// Apply a skew to a vertex of a polygon to give a pseudo-3D effect to the node shape
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="center"></param>
    /// <param name="skewFactor"></param>
    /// <param name="rotationRadians"></param>
    /// <returns></returns>
    private static (double X, double Y) ApplyVertexPerspectiveSkew((double X, double Y) vertex,
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

    public void SetShapeConfiguration(DirectedGraphNode node,
                                      double? skewFactor = null)
    {
        if (skewFactor == null || skewFactor.Value == 0)
        {
            double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

            for (int i = 0; i < _numberOfSides; i++)
            {
                double angle = (2 * Math.PI / _numberOfSides * i) + rotationAngle;

                (double X, double Y) polygonVertex = (node.Position.X + node.Shape.Radius * Math.Cos(angle),
                                                      node.Position.Y + node.Shape.Radius * Math.Sin(angle));

                _shapeConfiguration.PolygonVertices.Add(polygonVertex);
            }
        }

        if (skewFactor != null && skewFactor.Value > 0 && _shapeConfiguration.PolygonVertices.Count > 0)
        {
            double rotationRadians = -0.785 + Random.Shared.NextDouble() * 1.57; // Range of -π/4 to π/2 radians

            for (int lcv = 0; lcv < _shapeConfiguration.PolygonVertices.Count; lcv++)
            {
                _shapeConfiguration.PolygonVertices[lcv] =
                    ApplyVertexPerspectiveSkew(_shapeConfiguration.PolygonVertices[lcv], node.Position, skewFactor.Value, rotationRadians);
            }
        }
    }

    public ShapeConfiguration GetShapeConfiguration()
    {
        return _shapeConfiguration;
    }
}