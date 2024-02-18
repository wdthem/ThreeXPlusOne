using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Interfaces.DirectedGraph;

public interface IShape
{
    /// <summary>
    /// The type of the shape (e.g. Ellipse or Polygon)
    /// </summary>
    ShapeType ShapeType { get; }

    /// <summary>
    /// The colour of the shape's border
    /// </summary>
    Color BorderColor { get; set; }

    /// <summary>
    /// The colour of the shape
    /// </summary>
    Color Color { get; set; }

    /// <summary>
    /// The radius of the shape
    /// </summary>
    double Radius { get; set; }

    /// <summary>
    /// Set the configuration of the given shape, optionally skewing it
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    /// <param name="skewFactor"></param>
    void SetShapeConfiguration((double X, double Y) nodePosition, double nodeRadius, double? skewFactor = null);

    /// <summary>
    /// Get the object storing the shape's configuration data
    /// </summary>
    /// <returns></returns>
    ShapeConfiguration GetShapeConfiguration();

    /// <summary>
    /// Set the configuration data for the node's halo
    /// </summary>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    void SetNodeHaloConfiguration(double radius, Color color);
}