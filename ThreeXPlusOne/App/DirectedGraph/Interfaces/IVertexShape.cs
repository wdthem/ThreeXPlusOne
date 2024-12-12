namespace ThreeXPlusOne.App.DirectedGraph.Interfaces;

/// <summary>
/// The interface for all Shape instances configured with vertices.
/// </summary>
public interface IVertexShape : IShape
{
    /// <summary>
    /// The vertices of the shape.
    /// </summary>
    List<(double X, double Y)> Vertices { get; }
}