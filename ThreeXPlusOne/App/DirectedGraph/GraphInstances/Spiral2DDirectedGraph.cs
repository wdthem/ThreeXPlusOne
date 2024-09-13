using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.GraphInstances;

public class Spiral2DDirectedGraph(IOptions<AppSettings> appSettings,
                                   IEnumerable<IDirectedGraphDrawingService> graphServices,
                                   ILightSourceService lightSourceService,
                                   IConsoleService consoleService,
                                   ShapeFactory shapeFactory)
                                       : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
                                         IDirectedGraph
{
    private readonly Dictionary<(int, int), List<(double X, double Y)>> _nodeGrid = [];

    private int _nodesPositioned = 0;

    public GraphType GraphType => GraphType.Spiral2D;

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
        // Set up the base node's position
        (double X, double Y) baseNodePosition = (0, 0);

        _nodes[1].Position = baseNodePosition;
        _nodes[1].IsPositioned = true;
        _nodes[1].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

        _nodesPositioned = 1;

        // Recursive method to position a node and its children
        PositionNode(_nodes[1], 10, 0, _nodes[1].Position.X, _nodes[1].Position.Y);

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
    /// Recursive method to position node and all its children down the tree.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <param name="centerX"></param>
    /// <param name="centerY"></param>
    private void PositionNode(DirectedGraphNode node,
                              float radius,
                              float angle,
                              double centerX,
                              double centerY)
    {
        _consoleService.Write($"\r{_nodesPositioned} nodes positioned... ");

        // Convert polar to Cartesian for the node
        double parentX = centerX + radius * (float)Math.Cos(angle * Math.PI / 180);
        double parentY = centerY + radius * (float)Math.Sin(angle * Math.PI / 180);

        // Assign the calculated position to the node
        node.Position = (parentX, parentY);
        node.IsPositioned = true;
        _nodesPositioned++;

        // Add node to grid for future overlap checks
        AddNodeToGrid(node, _appSettings.NodeAestheticSettings.NodeRadius);

        int nearbyNodeCount = CountNearbyNodes(node, _appSettings.NodeAestheticSettings.NodeRadius);
        float densityFactor = 1 + (nearbyNodeCount * 0.60f);  // Increase by percentage per nearby node
        float adjustedRadius = radius + (10 * densityFactor);

        // If there's a first child, continue the spiral
        DirectedGraphNode? firstChild = node.Children.FirstOrDefault(n => n.IsFirstChild);
        if (firstChild != null)
        {
            float newAngle = (float)(angle + _appSettings.DirectedGraphAestheticSettings.SpiralAngle);  //controls the angle of the spiral
            PositionNode(firstChild, adjustedRadius, newAngle, parentX, parentY);
        }

        // If there's a second child, start a new spiral
        DirectedGraphNode? secondChild = node.Children.FirstOrDefault(n => !n.IsFirstChild);
        if (secondChild != null)
        {
            double offsetX = parentX + adjustedRadius * 2;  // Dynamic offset based on radius
            double offsetY = parentY + adjustedRadius * 2;

            float newRadius = 10;
            float newAngle = 0;

            PositionNode(secondChild, newRadius, newAngle, offsetX, offsetY);
        }
    }

    /// <summary>
    /// Determine the number of neighbouring nodes within a certain distance.
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    private int CountNearbyNodes(DirectedGraphNode newNode,
                                 double minDistance)
    {
        (int, int) cell = GetGridCellForNode(newNode, minDistance);
        int nearbyNodeCount = 0;

        // Check this cell and adjacent cells
        foreach ((int, int) offset in new[] { (0, 0), (1, 0), (0, 1), (-1, 0), (0, -1) })
        {
            (int, int) checkCell = (cell.Item1 + offset.Item1, cell.Item2 + offset.Item2);

            if (_nodeGrid.TryGetValue(checkCell, out var nodesInCell))
            {
                foreach ((double X, double Y) node in nodesInCell)
                {
                    if (Distance(newNode.Position, node) < minDistance)
                    {
                        nearbyNodeCount++;
                    }
                }
            }
        }

        return nearbyNodeCount;
    }


    /// <summary>
    /// Retrieve the cell in the grid object in which the node is positioned.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    private static (int, int) GetGridCellForNode(DirectedGraphNode node,
                                                 double cellSize)
    {
        return ((int)(node.Position.X / cellSize), (int)(node.Position.Y / cellSize));
    }

    /// <summary>
    /// Add the node to the grid dictionary to keep track of node positions via a grid system.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="minDistance"></param>
    private void AddNodeToGrid(DirectedGraphNode node,
                               double minDistance)
    {
        (int, int) cell = GetGridCellForNode(node, minDistance);

        if (!_nodeGrid.TryGetValue(cell, out List<(double X, double Y)>? value))
        {
            value = ([]);
            _nodeGrid[cell] = value;
        }

        value.Add(node.Position);
    }
}