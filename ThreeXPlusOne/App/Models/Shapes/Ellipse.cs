using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces;

namespace ThreeXPlusOne.App.Models.Shapes;

/// <summary>
/// For Elliptical shapes, which includes Circles because they are a special-case ellipse
/// </summary>
public class Ellipse() : Shape, IShape
{
    private readonly ShapeConfiguration _shapeConfiguration = new();

    public ShapeType ShapeType => ShapeType.Ellipse;

    public void SetShapeConfiguration(DirectedGraphNode node,
                                      double? skewFactor = null)
    {
        double horizontalOffset = 0;

        if (skewFactor != null && skewFactor.Value > 0)
        {
            skewFactor *= 0.6;  //reduce the skew impact for ellipses
            horizontalOffset = node.Shape.Radius * skewFactor.Value;
        }

        double horizontalRadius = node.Shape.Radius + horizontalOffset;
        double verticalRadius = node.Shape.Radius;

        _shapeConfiguration.EllipseConfig = ((node.Position.X, node.Position.Y),
                                             horizontalRadius,
                                             verticalRadius);
    }

    public ShapeConfiguration GetShapeConfiguration()
    {
        return _shapeConfiguration;
    }
}