using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph.Services;

public class OpenTKGraphService(IConsoleHelper consoleHelper) : IGraphService
{
    public GraphProvider GraphProvider => GraphProvider.OpenTK;

    public void InitializeGraph(int width, int height)
    {
        consoleHelper.WriteLine("");
    }

    public void DrawConnection(DirectedGraphNode node)
    {

    }

    public void DrawNode(DirectedGraphNode node, bool drawNumbersOnNodes, bool distortNodes)
    {

    }

    public void GenerateBackgroundStars(int starCount)
    {

    }

    public void SaveGraphImage()
    {
        throw new NotImplementedException("Saving 3D graph not supported");
    }
}