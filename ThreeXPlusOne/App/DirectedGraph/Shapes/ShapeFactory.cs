using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public class ShapeFactory(IEnumerable<IShape> shapes)
{
    private List<KeyValuePair<ShapeType, ShapeSelectionWeight>>? _shapeSelectionWeights;
    private int _totalWeight = 0;

    /// <summary>
    /// Prepare the data required for weighting the selection of shapes
    /// </summary>
    /// <returns></returns>
    private List<KeyValuePair<ShapeType, ShapeSelectionWeight>> ConfigureShapeSelectionWeights()
    {
        List<KeyValuePair<ShapeType, ShapeSelectionWeight>> shapeWeightsList =
            shapes.Select(shape => new KeyValuePair<ShapeType, ShapeSelectionWeight>
                                        (
                                            shape.ShapeType,
                                            new ShapeSelectionWeight() { Weight = shape.SelectionWeight }
                                        ))
                  .ToList();

        int totalWeight = 0;

        foreach (KeyValuePair<ShapeType, ShapeSelectionWeight> pair in shapeWeightsList)
        {
            totalWeight += pair.Value.Weight;
            pair.Value.CumulativeWeight = totalWeight;
        }

        return shapeWeightsList;
    }

    /// <summary>
    /// Return either the passed-in ShapeType or a randomly-selected shape, biased toward defined weightings
    /// </summary>
    /// <param name="shapeType"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private ShapeType SelectShapeType(ShapeType? shapeType)
    {
        if (shapeType != null)
        {
            return shapeType.Value;
        }

        if (_shapeSelectionWeights == null)
        {
            _shapeSelectionWeights = ConfigureShapeSelectionWeights();
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
    public IShape CreateShape(ShapeType? shapeType = null)
    {
        shapeType = SelectShapeType(shapeType);

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

    /// <summary>
    /// Local model class for helping to weight the selection of shapes
    /// </summary>
    private class ShapeSelectionWeight
    {
        /// <summary>
        /// The weight of the shape, as defined in the individual shape class
        /// </summary>
        public int Weight { get; set; }

        /// <summary>
        /// The running total of the given shape's weight plus all weights before it
        /// </summary>
        public int CumulativeWeight { get; set; }
    }
}