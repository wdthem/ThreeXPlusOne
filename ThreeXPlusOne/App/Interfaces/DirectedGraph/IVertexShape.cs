namespace ThreeXPlusOne.App.Interfaces.DirectedGraph;

public interface IVertexShape
{
    /// <summary>
    /// The vertices of the shape
    /// </summary>
    List<(double X, double Y)> Vertices { get; }
}