namespace ThreeXPlusOne.App.Models;

public record ShapeConfiguration()
{
    /// <summary>
    /// A list of all the vertices of a polygon
    /// </summary>
    public List<(double X, double Y)> PolygonVertices { get; set; } = [];

    /// <summary>
    /// The coordinates and radii of an ellipse
    /// </summary>
    public EllipseConfiguration? EllipseConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing an arc shape
    /// </summary>
    public ArcConfiguration? ArcConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing a pill shape
    /// </summary>
    public PillConfiguration? PillConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing a semicircle shape
    /// </summary>
    public SemiCircleConfiguration? SemiCircleConfiguration { get; set; }
}