using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

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
    /// Whether or not the shape has a gap when it is drawn (e.g. the gap between the bottom parts of an arc)
    /// </summary>
    bool HasGap { get; }

    /// <summary>
    /// The radius of the shape
    /// </summary>
    double Radius { get; set; }

    /// <summary>
    /// The color to use to start the gradient on the side of the shape in pseudo-3D
    /// </summary>
    public Color ThreeDimensionalSideGradientStartColor { get; set; }

    /// <summary>
    /// The color to use to end the gradient on the side of the shape in pseudo-3D
    /// </summary>
    public Color ThreeDimensionalSideGradientEndColor { get; }

    /// <summary>
    /// The weight assigned to the given shape with respect to it being randomly selected as the shape for the node
    /// <remarks>
    /// Used to bias the selection toward shapes with higher weights
    /// </remarks>
    /// </summary>
    int SelectionWeight { get; }

    /// <summary>
    /// Set the configuration of the given shape, optionally skewing it
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    void SetShapeConfiguration((double X, double Y) nodePosition, double nodeRadius);

    /// <summary>
    /// Get the object storing the shape's configuration data
    /// </summary>
    /// <returns></returns>
    ShapeConfiguration GetShapeConfiguration();

    /// <summary>
    /// Set the start and end of the gradient of the front and sides of the 3D shape
    /// </summary>
    /// <param name="frontFaceStartPoint"></param>
    /// <param name="frontFaceEndPoint"></param>
    /// <param name="sideStartPoint"></param>
    /// <param name="sideEndPoint"></param>
    void SetNodeThreeDimensionalGradientPoints((double X, double Y) frontFaceStartPoint,
                                               (double X, double Y) frontFaceEndPoint,
                                               (double X, double Y) sideStartPoint,
                                               (double X, double Y) sideEndPoint);

    /// <summary>
    /// Assign random skew values to the shape
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    void SetShapeSkew((double X, double Y) nodePosition, double nodeRadius);
}