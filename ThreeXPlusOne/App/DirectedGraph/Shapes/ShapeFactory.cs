using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;
public class ShapeFactory(IEnumerable<IShape> shapes)
{
    private List<KeyValuePair<ShapeType, ShapeSelectionWeight>>? _shapeSelectionWeights;
    private int _totalWeight = 0;

    /// <summary>
    /// Return either the passed-in ShapeType or a randomly-selected shape, biased toward defined weightings
    /// </summary>
    /// <param name="shapeType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private ShapeType SelectShapeType(List<ShapeType>? shapeTypes)
    {
        List<IShape> shapesList = [];

        if (shapeTypes != null)
        {
            if (shapeTypes.Count == 1)
            {
                return shapeTypes[0];
            }

            shapesList = shapes.Where(shape => shapeTypes.Contains(shape.ShapeType)).ToList();
        }

        if (shapesList.Count == 0)
        {
            shapesList = shapes.ToList();
        }

        if (_shapeSelectionWeights == null)
        {
            _shapeSelectionWeights = ShapeHelper.ConfigureShapeSelectionWeights(shapesList);
            _totalWeight = _shapeSelectionWeights.Sum(pair => pair.Value.Weight);
        }

        int randomNumber = Random.Shared.Next(1, _totalWeight + 1);

        ShapeType selectedShapeType =
            _shapeSelectionWeights.FirstOrDefault(pair => randomNumber <= pair.Value.CumulativeWeight).Key;

        return selectedShapeType;
    }

    /// <summary>
    /// Create either the ShapeType specified or a randomly-selected ShapeType
    /// </summary>
    /// <param name="shapeType"></param>
    /// <returns></returns>
    public IShape CreateShape(List<ShapeType>? shapeTypes = null)
    {
        ShapeType shapeType = SelectShapeType(shapeTypes);

        return shapeType switch
        {
            ShapeType.Ellipse => new Ellipse(),
            ShapeType.Polygon => new Polygon(),
            ShapeType.SemiCircle => new SemiCircle(),
            ShapeType.Arc => new Arc(),
            ShapeType.Pill => new Pill(),
            ShapeType.Star => new Star(),
            ShapeType.Seashell => new Seashell(),
            _ => throw new ArgumentException($"Unsupported ShapeType in ShapeFactory.CreateShape(): {shapeType}"),
        };
    }
}