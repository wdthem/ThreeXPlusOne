namespace ThreeXPlusOne.App.Models;

/// <summary>
/// Local model class for helping to weight the selection of shapes
/// </summary>
public class ShapeSelectionWeight
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