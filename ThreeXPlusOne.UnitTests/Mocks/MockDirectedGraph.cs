using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.UnitTests.Mocks;

public class MockDirectedGraph(IOptions<Settings> settings,
                               IEnumerable<IGraphService> graphServices,
                               IConsoleHelper consoleHelper) : DirectedGraph(settings, graphServices, consoleHelper), IDirectedGraph
{
    public int Dimensions => 2;

    public void SetCanvasDimensions()
    {
    }

    public void Draw()
    {
    }

    public void PositionNodes()
    {
    }

    public (float x, float y) RotateNode_Base(int nodeValue, float rotationAngle, float x, float y)
    {
        return RotateNode(nodeValue, rotationAngle, x, y);
    }
}