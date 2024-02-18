using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph;
using ThreeXPlusOne.App.DirectedGraph.Shapes;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.UnitTests.Mocks;

public class MockDirectedGraph(IOptions<AppSettings> appSettings,
                               IEnumerable<IDirectedGraphService> graphServices,
                               ILightSourceService lightSourceService,
                               IConsoleService consoleService,
                               ShapeFactory shapeFactory)
                                    : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
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

    public void SetNodeAesthetics()
    {
    }

#pragma warning disable CA1822 // Mark members as static
    public (double x, double y) RotateNode_Base(int nodeValue, double rotationAngle, double x, double y)
    {
        return NodePositions.RotateNode(nodeValue, rotationAngle, x, y);
    }
#pragma warning restore CA1822 // Mark members as static

    public void DrawDirectedGraph_Base()
    {
        DrawDirectedGraph();
    }
}