namespace ThreeXPlusOne.App.Models;

public class SpiralDirectedGraphNode(int numberValue) : DirectedGraphNode(numberValue)
{
    /// <summary>
    /// The center of the spiral to which the node belongs.
    /// </summary>
    public (double X, double Y) SpiralCenter { get; set; }
}