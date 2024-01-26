using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.UnitTests.Mocks;

public class MockDirectedGraph(IOptions<Settings> settings,
                               IEnumerable<IDirectedGraphService> graphServices,
                               ILightSourceService lightSourceService,
                               IConsoleHelper consoleHelper) : DirectedGraph(settings, graphServices, lightSourceService, consoleHelper), IDirectedGraph
{
    private new readonly NodeAesthetics _nodeAesthetics = new();

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

    public void SetNodeShapes()
    {
    }

    public (double x, double y) RotateNode_Base(int nodeValue, double rotationAngle, double x, double y)

    {
        return _nodeAesthetics.RotateNode(nodeValue, rotationAngle, x, y);
    }

    public void DrawDirectedGraph_Base()
    {
        DrawDirectedGraph();
    }
}