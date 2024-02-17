namespace ThreeXPlusOne.App.Models;

public record ArcConfiguration
{
    /// <summary>
    /// 
    /// </summary>
    public double StartAngle { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public double SweepAngle { get; set; }

    /// <summary>
    /// The radius of the inner arc
    /// </summary>
    public double InnerRadius { get; set; }

    /// <summary>
    /// The radius of the outer arc
    /// </summary>
    public double OuterRadius { get; set; }

    /// <summary>
    /// Skew values applied to the arc shape in psuedo-3D graphs
    /// </summary>
    public (double X, double Y) Skew { get; set; }
}