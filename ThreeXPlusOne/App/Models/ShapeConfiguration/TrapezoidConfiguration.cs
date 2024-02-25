namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record TrapezoidConfiguration
{
    /// <summary>
    /// The top width of the trapezoid
    /// </summary>
    /// <remarks>
    /// The bottom width is determined by the node's radius
    /// </remarks>
    public double TopWidth { get; set; }

    /// <summary>
    /// The position of the top left vertex
    /// </summary>
    public (double X, double Y) TopLeftVertex { get; set; }

    /// <summary>
    /// The position of the top right vertex
    /// </summary>
    public (double X, double Y) TopRightVertex { get; set; }

    /// <summary>
    /// The position of the bottom right vertex
    /// </summary>
    public (double X, double Y) BottomRightVertex { get; set; }

    /// <summary>
    /// The position of the bottom left vertex
    /// </summary>
    public (double X, double Y) BottomLeftVertex { get; set; }
}