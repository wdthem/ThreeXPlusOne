namespace ThreeXPlusOne.App.Models;

public record StarConfiguration
{
    /// <summary>
    /// The angle increment determines the angular separation between each point of the star
    /// </summary>
    public double AngleIncrement { get; set; }

    /// <summary>
    /// The radius of the indentations between tips
    /// </summary>
    /// <remarks>
    /// The outer radius is determined by the node radius
    /// </remarks>
    public double InnerRadius { get; set; }

    /// <summary>
    /// The coordinates of the stars angle vertices
    /// </summary>
    public List<(double X, double Y)> AngleVertices { get; set; } = [];
}