namespace ThreeXPlusOne.App.Models;

public record SemiCircleConfiguration
{
    /// <summary>
    /// Specifies the angle direction in which the flat side of the semicircle is facing.
    /// </summary>
    public double Orientation { get; set; }

    /// <summary>
    /// Skew values applied to the pill shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y)? Skew { get; set; }
}