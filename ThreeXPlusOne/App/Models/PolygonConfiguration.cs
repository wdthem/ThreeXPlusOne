namespace ThreeXPlusOne.App.Models;

public record PolygonConfiguration
{
    /// <summary>
    /// The coordinates of the polygon's vertices
    /// </summary>
    public List<(double X, double Y)> Vertices { get; set; } = [];

    /// <summary>
    /// Skew values applied to the arc shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y)? Skew { get; set; }
}