namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record DonutConfiguration
{
    /// <summary>
    /// The x-radius of the donut
    /// </summary>
    public double RadiusX { get; set; }

    /// <summary>
    /// The y-radius of the donut
    /// </summary>
    public double RadiusY { get; set; }

    /// <summary>
    /// The bounding box used to render the outside of the donut shape
    /// </summary>
    public ShapeBounds OuterShapeBounds { get; set; } = new();

    /// <summary>
    /// The bounding box used to render the inside of the donut shape
    /// </summary>
    public ShapeBounds InnerShapeBounds { get; set; } = new();
}