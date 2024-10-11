using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.UnitTests.Mocks;

public class MockDirectedGraph(IOptions<AppSettings> appSettings,
                               IEnumerable<IDirectedGraphDrawingService> graphServices,
                               ILightSourceService lightSourceService,
                               IConsoleService consoleService,
                               ShapeFactory shapeFactory)
                                    : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
                                      IDirectedGraph
{
    public GraphType GraphType => GraphType.Standard2D;

    public void SetCanvasDimensions()
    {
    }

    public async Task Draw()
    {
        await Task.CompletedTask;
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

    public void TranslateNodesToPositiveCoordinates_Base(Dictionary<int, DirectedGraphNode> nodes,
                                                         double xNodeSpacer,
                                                         double yNodeSpacer,
                                                         double nodeRadius)
    {
        NodePositions.TranslateNodesToPositiveCoordinates(nodes, xNodeSpacer, yNodeSpacer, nodeRadius);
    }
#pragma warning restore CA1822 // Mark members as static

    public async Task DrawDirectedGraph_Base()
    {
        await DrawDirectedGraph();
    }
}