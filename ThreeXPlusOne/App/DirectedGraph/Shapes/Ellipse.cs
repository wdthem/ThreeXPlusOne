using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

/// <summary>
/// For Elliptical shapes, which includes Circles because they are a special-case ellipse
/// </summary>
public class Ellipse() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Ellipse;

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
        double horizontalOffset = 0;
        double verticalOffset = 0;

        if (skewFactor != null && skewFactor.Value > 0)
        {
            skewFactor *= 0.6;  //reduce the skew impact for ellipses
            horizontalOffset = nodeRadius * skewFactor.Value;
        }

        double horizontalRadius = nodeRadius + horizontalOffset;
        double verticalRadius = nodeRadius + verticalOffset;

        _shapeConfiguration.EllipseConfiguration = new()
        {
            RadiusX = horizontalRadius,
            RadiusY = verticalRadius
        };
    }
}