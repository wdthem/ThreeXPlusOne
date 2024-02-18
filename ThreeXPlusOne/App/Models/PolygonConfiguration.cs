namespace ThreeXPlusOne.App.Models;

public record PolygonConfiguration
{
    /// <summary>
    /// The coordinates of the polygon's vertices
    /// </summary>
    public List<(double X, double Y)> Vertices { get; set; } = [];
}