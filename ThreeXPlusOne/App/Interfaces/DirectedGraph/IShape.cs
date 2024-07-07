using System.Drawing;
using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.App.Interfaces.DirectedGraph;

/// <summary>
/// The common interface for all Shape instances.
/// </summary>
public interface IShape
{
    /// <summary>
    /// The type of the shape (e.g. Ellipse or Polygon).
    /// </summary>
    ShapeType ShapeType { get; }

    /// <summary>
    /// The colour of the shape's border.
    /// </summary>
    Color BorderColor { get; set; }

    /// <summary>
    /// The colour of the shape.
    /// </summary>
    Color Color { get; set; }

    /// <summary>
    /// Whether or not the shape has a gap when it is drawn (e.g. the gap between the bottom parts of an arc).
    /// </summary>
    bool HasGap { get; }

    /// <summary>
    /// The dimensions of the shape (2 or 3).
    /// </summary>
    int Dimensions { get; }

    /// <summary>
    /// The color to use to start the gradient if the node is influenced by the light source.
    /// </summary>
    Color GradientStartColor { get; set; }

    /// <summary>
    /// The color to use to end the gradient if the node is influenced by the light source.
    /// </summary>
    Color GradientEndColor { get; }

    /// <summary>
    /// The color to use to start the gradient if the node is influenced by the light source.
    /// </summary>
    Color BorderGradientStartColor { get; set; }

    /// <summary>
    /// The color to use to end the gradient if the node is influenced by the light source.
    /// </summary>
    Color BorderGradientEndColor { get; }

    /// <summary>
    /// Whether or not this shape is influenced by the light source.
    /// </summary>
    bool HasLightSourceImpact { get; set; }

    /// <summary>
    /// The start point of the gradient for the shape's front face.
    /// </summary>
    (double X, double Y) FrontFaceGradientStartPoint { get; set; }

    /// <summary>
    /// The end point of the gradient for the shape's front face.
    /// </summary>
    (double X, double Y) FrontFaceGradientEndPoint { get; set; }

    /// <summary>
    /// The radius of the shape.
    /// </summary>
    double Radius { get; set; }

    /// <summary>
    /// The weight assigned to the given shape with respect to it being randomly selected as the shape for the node.
    /// <remarks>
    /// Used to bias the selection toward shapes with higher weights.
    /// </remarks>
    /// </summary>
    int SelectionWeight { get; }

    /// <summary>
    /// The start point of the gradient for the shape's side.
    /// </summary>
    (double X, double Y) SideFaceGradientStartPoint { get; set; }

    /// <summary>
    /// The end point of the gradient for the shape's side.
    /// </summary>
    (double X, double Y) SideFaceGradientEndPoint { get; set; }

    /// <summary>
    /// Skew values applied to the shape in psuedo-3D graphs.
    /// </summary>
    (double X, double Y)? Skew { get; set; }

    /// <summary>
    /// The number of sides to render when drawing the shape in pseudo-3D.
    /// </summary>
    int ThreeDimensionalSideCount { get; }

    /// <summary>
    /// Set the configuration of the given shape, optionally skewing it.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    void SetShapeConfiguration((double X, double Y) nodePosition, double nodeRadius);

    /// <summary>
    /// Set the start and end of the gradient of the front and sides of the shape.
    /// </summary>
    /// <param name="frontFaceStartPoint"></param>
    /// <param name="frontFaceEndPoint"></param>
    /// <param name="sideStartPoint"></param>
    /// <param name="sideEndPoint"></param>
    void SetNodeGradientPoints((double X, double Y) frontFaceStartPoint,
                               (double X, double Y) frontFaceEndPoint,
                               (double X, double Y) sideStartPoint,
                               (double X, double Y) sideEndPoint);

    /// <summary>
    /// Assign random skew values to the shape.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="nodeRadius"></param>
    void SetShapeSkew((double X, double Y) nodePosition, double nodeRadius);

    /// <summary>
    /// The depth of the shape when rendered in pseudo-3D, based on the node radius.
    /// </summary>
    /// <param name="nodeRadius"></param>
    /// <returns></returns>
    double ThreeDimensionalDepth(double nodeRadius);
}