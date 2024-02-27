namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record PlusConfiguration
{
    /// <summary>
    /// The vertices of the shape
    /// </summary>
    public List<(double X, double Y)> Vertices { get; set; } = [];
}