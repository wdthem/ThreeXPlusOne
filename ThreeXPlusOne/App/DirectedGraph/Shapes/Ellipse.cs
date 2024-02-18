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
        //adding skew
        //For ellipses, stretch radii in addition to adding skew
        if (_shapeConfiguration.EllipseConfiguration != null)
        {
            double horizontalOffset = 0;
            double verticalOffset = 0;

            if (skewFactor != null && skewFactor.Value != 0)
            {
                skewFactor *= 0.6;  //reduce the skew impact for ellipses
                horizontalOffset = nodeRadius * skewFactor.Value;
                verticalOffset = nodeRadius * (skewFactor.Value * Random.Shared.NextDouble());
            }

            _shapeConfiguration.EllipseConfiguration.RadiusX = nodeRadius + horizontalOffset;
            _shapeConfiguration.EllipseConfiguration.RadiusY = nodeRadius + verticalOffset;
            _shapeConfiguration.EllipseConfiguration.Skew = GetShapeSkew(skewFactor);

            return;
        }

        _shapeConfiguration.EllipseConfiguration = new()
        {
            RadiusX = nodeRadius,
            RadiusY = nodeRadius
        };
    }
}