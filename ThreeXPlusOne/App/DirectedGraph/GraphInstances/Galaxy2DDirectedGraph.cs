using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.GraphInstances;

public class Galaxy2DDirectedGraph(IOptions<AppSettings> appSettings,
                                   IEnumerable<IDirectedGraphDrawingService> graphServices,
                                   ILightSourceService lightSourceService,
                                   IConsoleService consoleService,
                                   ShapeFactory shapeFactory)
                                      : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
                                        IDirectedGraph
{
    private int _nodesPositioned = 0;

    public GraphType GraphType => GraphType.Galaxy2D;

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
        int layerSpacing = _appSettings.DirectedGraphAestheticSettings.RadialLayerSpacing;

        // Set up a spiral factor to create the curve for the first children
        float spiralFactor = 5.0f;  // Controls how tight the curve/spiral is

        // Set the rotation angles (tilt the galaxy in 3D)
        double tiltXAngle = 60.0;  // Tilt along the X-axis
        double tiltYAngle = 30.0;  // Tilt along the Y-axis (create a more angled view)
        double tiltXAngleRadians = tiltXAngle * Math.PI / 180.0;
        double tiltYAngleRadians = tiltYAngle * Math.PI / 180.0;

        (double x, double y) spiralCenter = (0, 0);

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

                // Handle first children in a spiraling fashion
                if (node.Parent != null && node.IsFirstChild)
                {
                    // Apply a curve or spiral effect for first children
                    double angleInRadians = ((i * angleIncrement) + (depth * spiralFactor)) * Math.PI / 180.0;

                    // Calculate the 3D position with an added Z axis (depth)
                    double nodeX = currentRadius * Math.Cos(angleInRadians);
                    double nodeY = currentRadius * Math.Sin(angleInRadians);
                    double nodeZ = depth * 3;  // Reduce Z-axis scaling for a flatter effect

                    // Apply the 3D tilt by rotating around both the X and Y axes
                    (double projectedX, double projectedY) = Apply3DTilt(nodeX, nodeY, nodeZ, tiltXAngleRadians, tiltYAngleRadians);

                    // Set the node's 2D position after projection
                    node.Position = (projectedX, projectedY);
                }
                else
                {
                    // Apply random jitter for other nodes
                    double angleInRadians = (i * angleIncrement + Random.Shared.NextDouble() * 5.0) * Math.PI / 180.0;

                    // Calculate the 3D position with Z axis for depth
                    double nodeX = currentRadius * Math.Cos(angleInRadians);
                    double nodeY = currentRadius * Math.Sin(angleInRadians);
                    double nodeZ = depth * 3;  // Reduce Z-axis scaling for a flatter effect

                    // Apply the 3D tilt by rotating around both the X and Y axes
                    (double projectedX, double projectedY) = Apply3DTilt(nodeX, nodeY, nodeZ, tiltXAngleRadians, tiltYAngleRadians);

                    // Set the node's 2D position after projection
                    node.Position = (projectedX, projectedY);
                }

                // Set the node's radius (optional)
                node.Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

                node.IsPositioned = true;

                _nodesPositioned++;

                if (i == 0)
                {
                    spiralCenter = node.Position;
                }

                node.SpiralCenter = spiralCenter;

                _consoleService.Write($"\r{_nodesPositioned} nodes positioned... ");
            }
        }

        _consoleService.WriteDone();

        // Translate all nodes to positive coordinates
        NodePositions.TranslateNodesToPositiveCoordinates(_nodes,
                                                          _appSettings.NodeAestheticSettings.NodeSpacerX,
                                                          _appSettings.NodeAestheticSettings.NodeSpacerY,
                                                          _appSettings.NodeAestheticSettings.NodeRadius);
    }

    // Helper function to apply 3D rotation around both X and Y axes and project 3D to 2D
    private static (double X, double Y) Apply3DTilt(double x, double y, double z, double tiltXAngle, double tiltYAngle)
    {
        // Rotate around the X-axis by the tilt angle (to get the "tilted" perspective)
        double rotatedY = y * Math.Cos(tiltXAngle) - z * Math.Sin(tiltXAngle);
        double rotatedZ = y * Math.Sin(tiltXAngle) + z * Math.Cos(tiltXAngle);

        // Now rotate around the Y-axis
        double rotatedX = x * Math.Cos(tiltYAngle) + rotatedZ * Math.Sin(tiltYAngle);

        // Project 3D (rotatedX, rotatedY) to 2D space
        return (rotatedX, rotatedY);
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