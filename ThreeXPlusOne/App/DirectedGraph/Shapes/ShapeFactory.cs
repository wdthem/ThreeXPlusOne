using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class ShapeFactory()
{
    private readonly ShapeType[] _shapeTypes = (ShapeType[])Enum.GetValues(typeof(ShapeType));

    /// <summary>
    /// Create either the ShapeType specified or a randomly-selected ShapeType
    /// </summary>
    /// <param name="shapeType"></param>
    /// <returns></returns>
    public IShape CreateShape(ShapeType? shapeType = null)
    {
        shapeType ??= _shapeTypes[Random.Shared.Next(_shapeTypes.Length)];

        return shapeType switch
        {
            ShapeType.Ellipse => new Ellipse(),
            ShapeType.Polygon => new Polygon(),
            ShapeType.SemiCircle => new SemiCircle(),
            ShapeType.Arc => new Arc(),
            ShapeType.Pill => new Pill(),
            ShapeType.Star => new Star(),
            _ => throw new ArgumentException($"Unsupported ShapeType in ShapeFactory.CreateShape(): {shapeType}"),
        };
    }
}