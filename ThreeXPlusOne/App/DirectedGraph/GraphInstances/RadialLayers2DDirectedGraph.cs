using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.GraphInstances;

public class RadialLayers2DDirectedGraph(IOptions<AppSettings> appSettings,
                                         IEnumerable<IDirectedGraphDrawingService> graphServices,
                                         ILightSourceService lightSourceService,
                                         IConsoleService consoleService,
                                         ShapeFactory shapeFactory)
                                            : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
                                              IDirectedGraph
{
    private int _nodesPositioned = 0;

    public GraphType GraphType => GraphType.RadialLayers2D;

    /// <summary>
    /// Assign sizes to the canvas width and height after having positioned the nodes.
    /// </summary>
    public void SetCanvasDimensions()
    {
        SetCanvasSize();
    }

    /// <summary>
    /// Generate a 2D visual representation of the directed graph.
    /// </summary>
    public void Draw()
    {
        DrawDirectedGraph();
    }

    /// <summary>
    /// Position the nodes on the graph in 2D space.
    /// </summary>
    public void PositionNodes()
    {
        // Group nodes by their depth (distance from root in Collatz tree)
        Dictionary<int, List<DirectedGraphNode>> nodesByDepth = GroupNodesByDepth(_nodes);
        int layerSpacing = 25;

        // Iterate over each depth level (layer)
        foreach (int depth in nodesByDepth.Keys)
        {
            List<DirectedGraphNode> nodesAtDepth = nodesByDepth[depth];
            int nodeCount = nodesAtDepth.Count;
            double angleIncrement = 360.0 / nodeCount;  // Evenly space nodes in the current layer

            // Calculate the radius for this layer
            double currentRadius = _appSettings.NodeAestheticSettings.NodeRadius + (depth * layerSpacing);

            for (int i = 0; i < nodeCount; i++)
            {
                DirectedGraphNode node = nodesAtDepth[i];

                // Calculate the angle for this node in radians
                double angleInRadians = (i * angleIncrement + Random.Shared.NextDouble() * 5.0) * Math.PI / 180.0;  // Add small jitter

                // Position the node on the circumference of the current layer
                double nodeX = 0 + currentRadius * Math.Cos(angleInRadians);
                double nodeY = 0 + currentRadius * Math.Sin(angleInRadians);

                node.Position = (nodeX, nodeY);

                // Set the node's radius (optional)
                node.Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

                node.IsPositioned = true;

                _nodesPositioned++;

                _consoleService.Write($"\r{_nodesPositioned} nodes positioned... ");
            }
        }

        _consoleService.WriteDone();

        NodePositions.TranslateNodesToPositiveCoordinates(_nodes,
                                                          _appSettings.NodeAestheticSettings.NodeSpacerX,
                                                          _appSettings.NodeAestheticSettings.NodeSpacerY,
                                                          _appSettings.NodeAestheticSettings.NodeRadius);
    }

    /// <summary>
    /// Set the shapes and colours of the positioned nodes.
    /// </summary>
    public void SetNodeAesthetics()
    {
        int lcv = 1;

        foreach (DirectedGraphNode node in _nodes.Values.Where(node => node.IsPositioned))
        {
            _nodeAesthetics.SetNodeShape(node,
                                         _appSettings.NodeAestheticSettings.NodeRadius,
                                         _appSettings.NodeAestheticSettings.NodeShapes);

            _nodeAesthetics.SetNodeColor(node,
                                         _appSettings.NodeAestheticSettings.NodeColors,
                                         _appSettings.NodeAestheticSettings.NodeColorsBias,
                                         _appSettings.NodeAestheticSettings.ColorCodeNumberSeries);

            _consoleService.Write($"\r{lcv} nodes styled... ");

            lcv++;
        }

        _consoleService.WriteDone();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="nodes"></param>
    /// <returns></returns>
    private static Dictionary<int, List<DirectedGraphNode>> GroupNodesByDepth(Dictionary<int, DirectedGraphNode> nodes)
    {
        var nodesByDepth = new Dictionary<int, List<DirectedGraphNode>>();

        foreach (var nodePair in nodes)
        {
            DirectedGraphNode node = nodePair.Value;
            int depth = node.Depth;  // Calculate how far the node is from the root

            if (!nodesByDepth.TryGetValue(depth, out List<DirectedGraphNode>? value))
            {
                value = ([]);
                nodesByDepth[depth] = value;
            }

            value.Add(node);
        }

        return nodesByDepth;
    }
}