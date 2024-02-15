using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.App.Interfaces;

public interface IShapeFactory
{
    /// <summary>
    /// Create either the ShapeType specified or a randomly-selected ShapeType
    /// </summary>
    /// <param name="shapeType"></param>
    /// <returns></returns>
    IShape CreateShape(ShapeType? shapeType = null);
}