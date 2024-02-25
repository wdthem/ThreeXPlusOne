using System.Drawing;

namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record ShapeConfiguration()
{
    /// <summary>
    /// The config data for drawing a polygon
    /// </summary>
    public PolygonConfiguration? PolygonConfiguration { get; set; }

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

    /// <summary>
    /// The config data for drawing a star shape
    /// </summary>
    public StarConfiguration? StarConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing a seashell shape
    /// </summary>
    public SeashellConfiguration? SeashellConfiguration { get; set; }

    /// <summary>
    /// The config data for drawing a trapezoid shape
    /// </summary>
    public TrapezoidConfiguration? TrapezoidConfiguration { get; set; }

    /// <summary>
    /// Skew values applied to the shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y)? Skew { get; set; }

    /// <summary>
    /// The radius and colour of the node's halo when a light source exists
    /// </summary>
    public (double Radius, Color Color)? HaloConfiguration { get; set; }
}