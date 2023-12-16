namespace ThreeXPlusOne.Code.Interfaces;

public interface IDirectedGraph
{
    int Dimensions { get; }
    void AddSeries(List<int> series);

    /// <summary>
    /// Position the nodes on the graph bassed on the provided settings
    /// </summary>
    void PositionNodes();

    /// <summary>
    /// Generate a visual representation of the graph based on the settings
    /// </summary>
    void DrawGraph();
}