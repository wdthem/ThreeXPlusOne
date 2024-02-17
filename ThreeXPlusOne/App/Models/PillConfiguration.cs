namespace ThreeXPlusOne.App.Models;

public record PillConfiguration
{
    /// <summary>
    /// The height of the pill shape
    /// </summary>
    /// <remarks>
    /// Width is determined by the node radius
    /// </remarks>
    public double Height { get; set; }

    /// <summary>
    /// The angle by which the shape is rotated on the graph
    /// </summary>
    public double RotationAngle { get; set; }

    /// <summary>
    /// Skew values applied to the pill shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y) Skew { get; set; }
}