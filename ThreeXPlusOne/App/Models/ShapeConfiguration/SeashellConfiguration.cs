namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record SeashellConfiguration
{
    /// <summary>
    /// The list of coordinates used to draw the spiral and outer edge of the seashell
    /// </summary>
    public List<(double X, double Y)> SpiralCoordinates { get; set; } = [];
}