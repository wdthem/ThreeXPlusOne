using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Arc() : Shape, IShape
{
    private readonly ShapeConfiguration _shapeConfiguration = new();

    public ShapeType ShapeType => ShapeType.Arc;

    /// <summary>
    /// Get the shape's configuration data
    /// </summary>
    /// <returns></returns>
    public ShapeConfiguration GetShapeConfiguration()
    {
        return _shapeConfiguration;
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
        _shapeConfiguration.ArcConfig = (Random.Shared.Next(0, 360),
                                         180,
                                         Random.Shared.Next(10, (int)nodeRadius));
    }
}