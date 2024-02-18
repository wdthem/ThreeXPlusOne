namespace ThreeXPlusOne.App.Models;

public record ArcConfiguration
{
    /// <summary>
    /// The angle in degrees at which the arc begins. Measured clockwise from the positive x-axis (3 o'clock position). 
    /// </summary>
    public double StartAngle { get; set; }

    /// <summary>
    /// The angle in degrees that the arc covers. Positive values indicate a clockwise sweep, while negative values indicate a counterclockwise sweep. 
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
    public (double X, double Y)? Skew { get; set; }
}