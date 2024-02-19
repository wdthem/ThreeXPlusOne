namespace ThreeXPlusOne.App.Models.ShapeConfiguration;

public record ArcConfiguration
{
    /// <summary>
    /// The angle in degrees at which the arc begins. Measured clockwise from the positive x-axis (3 o'clock position). 
    /// </summary>
    public double TopArcStartAngle { get; set; }

    /// <summary>
    /// The angle in degrees at which the arc begins. Measured clockwise from the positive x-axis (3 o'clock position). 
    /// </summary>
    public double BottomArcStartAngle { get; set; }

    /// <summary>
    /// The angle in degrees that the arc covers. Positive values indicate a clockwise sweep, while negative values indicate a counterclockwise sweep. 
    /// </summary>
    public double TopArcSweepAngle { get; set; }

    /// <summary>
    /// The angle in degrees that the arc covers. Positive values indicate a clockwise sweep, while negative values indicate a counterclockwise sweep. 
    /// </summary>
    public double BottomArcSweepAngle { get; set; }

    /// <summary>
    /// The bounding box used to render the top arc shape
    /// </summary>
    public ShapeBounds TopArcBounds { get; set; } = new();

    /// <summary>
    /// The bounding box used to render the top arc shape
    /// </summary>
    public ShapeBounds BottomArcBounds { get; set; } = new();
}