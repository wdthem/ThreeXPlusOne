using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.UnitTests.Mocks;

public class MockDirectedGraph(IOptions<Settings> settings,
                               IEnumerable<IDirectedGraphService> graphServices,
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

    //ignore warning because this is called from a unit test instance
#pragma warning disable CA1822 // Mark members as static

    public (float x, float y) RotateNode_Base(int nodeValue, float rotationAngle, float x, float y)

    {
        return RotateNode(nodeValue, rotationAngle, x, y);
    }

#pragma warning restore CA1822 // Mark members as static

    public void DrawDirectedGraph_Base()
    {
        DrawDirectedGraph();
    }
}