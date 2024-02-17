namespace ThreeXPlusOne.App.Models;

public record SemiCircleConfiguration
{
    /// <summary>
    /// The angle orientation of the semicircle
    /// </summary>
    public double Orientation { get; set; }

    /// <summary>
    /// Skew values applied to the pill shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y) Skew { get; set; }
}