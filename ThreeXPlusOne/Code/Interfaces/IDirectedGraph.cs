namespace ThreeXPlusOne.Code.Interfaces;

public interface IDirectedGraph
{
    int Dimensions { get; }
    void AddSeries(List<int> series);
    void PositionNodes();
    void DrawGraph();
}