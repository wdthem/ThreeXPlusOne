using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Models.Shapes;

namespace ThreeXPlusOne.App.Factories;

public class ShapeFactory(IEnumerable<IShape> shapes) : IShapeFactory
{
    private readonly List<IShape> _shapesList = shapes.ToList();

    /// <summary>
    /// Create either the shape specified or a randomly-selected ShapeType
    /// </summary>
    /// <param name="shapeType"></param>
    /// <returns></returns>
    public IShape CreateShape(ShapeType? shapeType = null)
    {
        shapeType ??= _shapesList[Random.Shared.Next(0, _shapesList.Count)].ShapeType;

        return shapeType switch
        {
            ShapeType.Ellipse => new Ellipse(),
            ShapeType.Polygon => new Polygon(),
            _ => throw new ArgumentException("Unsupported ShapeType passed to ShapeFactory"),
        };
    }
}