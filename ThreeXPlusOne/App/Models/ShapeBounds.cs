namespace ThreeXPlusOne.App.Models;

public record ShapeBounds
{
    /// <summary>
    ///  The x-coordinate of the left edge of the rectangle
    /// </summary>
    public double Left { get; set; }

    /// <summary>
    /// The y-coordinate of the top edge of the rectangle
    /// </summary>
    public double Top { get; set; }

    /// <summary>
    /// The x-coordinate of the right edge of the rectangle
    /// </summary>
    public double Right { get; set; }

    /// <summary>
    /// The y-coordinate of the bottom edge of the rectangle
    /// </summary>
    public double Bottom { get; set; }
}