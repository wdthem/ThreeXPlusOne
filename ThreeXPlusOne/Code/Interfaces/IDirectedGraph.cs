using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code.Interfaces;

public interface IDirectedGraph
{
    void AddSeries(List<int> series);
    void PositionNodes();
    void Draw(Settings settings);
}