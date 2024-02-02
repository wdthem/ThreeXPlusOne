using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.UnitTests.Mocks;

public class MockDirectedGraph(IOptions<Settings> settings,
                               IEnumerable<IDirectedGraphService> graphServices,
                               ILightSourceService lightSourceService,
                               IConsoleService consoleService)
                                    : DirectedGraph(settings, graphServices, lightSourceService, consoleService),
                                      IDirectedGraph
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

    public void SetNodeShapes()
    {
    }

    public (double x, double y) RotateNode_Base(int nodeValue, double rotationAngle, double x, double y)

    {
        return NodeAesthetics.RotateNode(nodeValue, rotationAngle, x, y);
    }

    public void DrawDirectedGraph_Base()
    {
        DrawDirectedGraph();
    }
}