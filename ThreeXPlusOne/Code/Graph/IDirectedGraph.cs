using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code.Graph;

public interface IDirectedGraph
{
    void AddSeries(List<int> series);
    void PositionNodes();
    void Draw(Settings settings);
}