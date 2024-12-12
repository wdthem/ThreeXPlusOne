using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.DirectedGraph.Interfaces;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.DirectedGraph.GraphInstances;

public class Spiral2DDirectedGraph(IOptions<AppSettings> appSettings,
                                   IEnumerable<IDirectedGraphDrawingService> graphServices,
                                   ILightSourceService lightSourceService,
                                   ShapeFactory shapeFactory,
                                   IDirectedGraphPresenter directedGraphPresenter)
                                       : DirectedGraph(appSettings, graphServices, lightSourceService, shapeFactory, directedGraphPresenter),
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
    public async Task Draw()
    {
        await DrawDirectedGraph();
    }

    /// <summary>
    /// Position the nodes on the graph in 2D space.
    /// </summary>
    public void PositionNodes()
    {
        if (_nodes[1] is not SpiralDirectedGraphNode node)
        {
            throw new Exception("The first node must be a SpiralDirectedGraphNode.");
        }

        // Set up the base node's position
        (double X, double Y) baseNodePosition = (0, 0);

        node.Position = baseNodePosition;
        node.IsPositioned = true;
        node.Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

        _nodesPositioned = 1;
        node.SpiralCenter = (0, 0);

        // Recursive method to position a node and its children
        PositionNode(node,
                     10,
                     0,
                     _nodes[1].Position.X,
                     _nodes[1].Position.Y);

        _directedGraphPresenter.DisplayDone();

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
                                         _appSettings.NodeAestheticSettings.ColorCodeNumberSeries);

            _directedGraphPresenter.DisplayNodesStyledMessage(lcv);

            lcv++;
        }

        _directedGraphPresenter.DisplayDone();
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="radius"></param>
    /// <param name="angle"></param>
    /// <param name="centerX"></param>
    /// <param name="centerY"></param>
    private void PositionNode(SpiralDirectedGraphNode node,
                              float radius,
                              float angle,
                              double centerX,
                              double centerY)
    {
        _directedGraphPresenter.DisplayNodesPositionedMessage(_nodesPositioned);

        // Convert polar to Cartesian for the node
        double parentX = centerX + radius * (float)Math.Cos(angle * Math.PI / 180);
        double parentY = centerY + radius * (float)Math.Sin(angle * Math.PI / 180);

        node.Position = (parentX, parentY);

        if (_appSettings.NodeAestheticSettings.NodeRotationAngle != 0)
        {
            (double x, double y) = NodePositions.RotateNode(node.NumberValue,
                                                            _appSettings.NodeAestheticSettings.NodeRotationAngle,
                                                            parentX,
                                                            parentY);

            node.Position = (x, y);
        }

        node.IsPositioned = true;
        _nodesPositioned++;

        // Add node to grid for future overlap checks
        AddNodeToGrid(node, _appSettings.NodeAestheticSettings.NodeRadius);

        int nearbyNodeCount = CountNearbyNodes(node, _appSettings.NodeAestheticSettings.NodeRadius);
        float densityFactor = 1 + (nearbyNodeCount * 0.60f);  // Increase by percentage per nearby node
        float adjustedRadius = radius + (10 * densityFactor);

        // If there's a first child, continue the spiral
        if (node.Children.FirstOrDefault(n => n.IsFirstChild) is SpiralDirectedGraphNode firstChild)
        {
            firstChild.SpiralCenter = node.SpiralCenter;

            float newAngle = (float)(angle + _appSettings.DirectedGraphInstanceSettings.SpiralAngle);  //controls the angle of the spiral
            PositionNode(firstChild, adjustedRadius, newAngle, parentX, parentY);
        }

        // If there's a second child, start a new spiral
        if (node.Children.FirstOrDefault(n => !n.IsFirstChild) is SpiralDirectedGraphNode secondChild)
        {
            double offsetX = parentX + adjustedRadius * 2;  // Dynamic offset based on radius
            double offsetY = parentY + adjustedRadius * 2;

            float newRadius = 10;
            float newAngle = 0;

            secondChild.SpiralCenter = (offsetX, offsetY);

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