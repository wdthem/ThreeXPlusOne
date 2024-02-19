namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record SemiCircleConfiguration
{
    private readonly double _sweepAngle = 180;

    /// <summary>
    /// Specifies the angle direction in which the flat side of the semicircle is facing.
    /// </summary>
    public double Orientation { get; set; }

    /// <summary>
    /// The angle in degrees that the semicircle covers. 
    /// </summary>
    /// <remarks>
    /// Constant value of 180 for a semicircle
    /// </remarks>
    public double SweepAngle => _sweepAngle;

    /// <summary>
    /// The bounding box used to render the semicircle shape
    /// </summary>
    public ShapeBounds ShapeBounds { get; set; } = new();
}