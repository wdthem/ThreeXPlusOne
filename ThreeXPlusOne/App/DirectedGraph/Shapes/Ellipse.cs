using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

/// <summary>
/// For Elliptical shapes, which includes Circles because they are a special-case ellipse
/// </summary>
public class Ellipse() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Ellipse;

    public int SelectionWeight => 1;

    /// <summary>
    /// Stretch the ellipse radii for skewed shapes
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    private void StretchRadii((double X, double Y) nodePosition,
                              double nodeRadius)
    {
        if (_shapeConfiguration.EllipseConfiguration != null)
        {
            double skewFactor = (Random.Shared.NextDouble() > 0.5 ? 1 : -1) *
                                ((0.1 + Random.Shared.NextDouble()) * 0.6) *
                                0.6;  //reduce the overall impact to ellipses

            double horizontalOffset = nodeRadius * skewFactor;
            double verticalOffset = nodeRadius * (skewFactor * Random.Shared.NextDouble());

            _shapeConfiguration.EllipseConfiguration.RadiusX = nodeRadius + horizontalOffset;
            _shapeConfiguration.EllipseConfiguration.RadiusY = nodeRadius + verticalOffset;

            _shapeConfiguration.EllipseConfiguration.ShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - _shapeConfiguration.EllipseConfiguration.RadiusX,
                Top = nodePosition.Y - _shapeConfiguration.EllipseConfiguration.RadiusY,
                Right = nodePosition.X + _shapeConfiguration.EllipseConfiguration.RadiusX,
                Bottom = nodePosition.Y + _shapeConfiguration.EllipseConfiguration.RadiusY
            };
        }
    }

    /// <summary>
    /// Set the configuration details for the shape used to represent the graph node
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    public void SetShapeConfiguration((double X, double Y) nodePosition,
                                      double nodeRadius)
    {
        _shapeConfiguration.EllipseConfiguration = new()
        {
            RadiusX = nodeRadius,
            RadiusY = nodeRadius,
            ShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - nodeRadius,
                Top = nodePosition.Y - nodeRadius,
                Right = nodePosition.X + nodeRadius,
                Bottom = nodePosition.Y + nodeRadius
            }
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
        StretchRadii(nodePosition, nodeRadius);

        GenerateShapeSkew();
    }
}