using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class Arc() : Shape, IShape
{
    public ShapeType ShapeType => ShapeType.Arc;

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
        //TODO: implement skewing

        _shapeConfiguration.ArcConfig = (Random.Shared.Next(0, 360),
                                         180,
                                         Random.Shared.Next((int)nodeRadius / 2, (int)nodeRadius));
    }
}