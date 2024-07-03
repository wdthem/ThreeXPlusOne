using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Donut() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Donut;

    public int SelectionWeight => 1;

    public bool HasGap => false;

    /// <summary>
    /// Stretch the ellipse radii for skewed shapes
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    private void StretchRadii((double X, double Y) nodePosition,
                              double nodeRadius)
    {
        if (_shapeConfiguration.DonutConfiguration != null)
        {
            double skewFactor = (Random.Shared.NextDouble() > 0.5 ? 1 : -1) *
                                ((0.1 + Random.Shared.NextDouble()) * 0.6) *
                                0.6;  //reduce the overall impact to ellipses

            double horizontalOffset = nodeRadius * skewFactor;
            double verticalOffset = nodeRadius * (skewFactor * Random.Shared.NextDouble());

            _shapeConfiguration.DonutConfiguration.RadiusX = nodeRadius + horizontalOffset;
            _shapeConfiguration.DonutConfiguration.RadiusY = nodeRadius + verticalOffset;

            _shapeConfiguration.DonutConfiguration.OuterShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - _shapeConfiguration.DonutConfiguration.RadiusX,
                Top = nodePosition.Y - _shapeConfiguration.DonutConfiguration.RadiusY,
                Right = nodePosition.X + _shapeConfiguration.DonutConfiguration.RadiusX,
                Bottom = nodePosition.Y + _shapeConfiguration.DonutConfiguration.RadiusY
            };

            _shapeConfiguration.DonutConfiguration.InnerShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - (_shapeConfiguration.DonutConfiguration.RadiusX / 2),
                Top = nodePosition.Y - (_shapeConfiguration.DonutConfiguration.RadiusY / 2),
                Right = nodePosition.X + (_shapeConfiguration.DonutConfiguration.RadiusX / 2),
                Bottom = nodePosition.Y + (_shapeConfiguration.DonutConfiguration.RadiusY / 2)
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
        _shapeConfiguration.DonutConfiguration = new()
        {
            RadiusX = nodeRadius,
            RadiusY = nodeRadius,
            OuterShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - nodeRadius,
                Top = nodePosition.Y - nodeRadius,
                Right = nodePosition.X + nodeRadius,
                Bottom = nodePosition.Y + nodeRadius
            },
            InnerShapeBounds = new ShapeBounds
            {
                Left = nodePosition.X - (nodeRadius / 2),
                Top = nodePosition.Y - (nodeRadius / 2),
                Right = nodePosition.X + (nodeRadius / 2),
                Bottom = nodePosition.Y + (nodeRadius / 2)
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