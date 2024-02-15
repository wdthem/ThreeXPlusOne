using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Models.Shapes;

namespace ThreeXPlusOne.App.Interfaces;

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
    /// The radius and colour of the node's halo when a light source exists
    /// </summary>
    (double Radius, Color Color) HaloConfig { get; set; }

    /// <summary>
    /// The radius of the shape
    /// </summary>
    double Radius { get; set; }

    /// <summary>
    /// Set the coordinates of the given shape, optionally skewing them
    /// </summary>
    /// <param name="node"></param>
    /// <param name="skewFactor"></param>
    void SetShapeConfiguration(DirectedGraphNode node, double? skewFactor = null);

    /// <summary>
    /// Get the object storing the shape's configuration data
    /// </summary>
    /// <returns></returns>
    ShapeConfiguration GetShapeConfiguration();
}