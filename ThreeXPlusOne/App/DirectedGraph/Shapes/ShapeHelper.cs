using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.Shapes;

public static class ShapeHelper
{
    /// <summary>
    /// Prepare the data required for weighting the selection of shapes
    /// </summary>
    /// <returns></returns>
    public static List<KeyValuePair<ShapeType, ShapeSelectionWeight>> ConfigureShapeSelectionWeights(List<IShape> shapes)
    {
        List<KeyValuePair<ShapeType, ShapeSelectionWeight>> shapeWeightsList =
            shapes.Select(shape => new KeyValuePair<ShapeType, ShapeSelectionWeight>
                                        (
                                            shape.ShapeType,
                                            new ShapeSelectionWeight() { Weight = shape.SelectionWeight }
                                        ))
                  .ToList();

        SetCumulativeShapeWeights(shapeWeightsList);

        return shapeWeightsList;
    }

    /// <summary>
    /// Prepare the data required for weighting the selection of a number of sides for a polygon
    /// </summary>
    /// <returns></returns>
    public static List<KeyValuePair<ShapeType, ShapeSelectionWeight>> ConfigureShapeSelectionWeights(int[] weights)
    {
        List<KeyValuePair<ShapeType, ShapeSelectionWeight>> shapeWeightsList =
            weights.Select(weight => new KeyValuePair<ShapeType, ShapeSelectionWeight>
                                        (
                                            ShapeType.Polygon,
                                            new ShapeSelectionWeight() { Weight = weight }
                                        ))
                          .ToList();

        SetCumulativeShapeWeights(shapeWeightsList);

        return shapeWeightsList;
    }

    /// <summary>
    /// Set cumulative weights for use in weighted random selection
    /// </summary>
    /// <param name="shapeWeightsList"></param>
    private static void SetCumulativeShapeWeights(List<KeyValuePair<ShapeType, ShapeSelectionWeight>> shapeWeightsList)
    {
        int totalWeight = 0;

        foreach (KeyValuePair<ShapeType, ShapeSelectionWeight> pair in shapeWeightsList)
        {
            totalWeight += pair.Value.Weight;
            pair.Value.CumulativeWeight = totalWeight;
        }
    }
}