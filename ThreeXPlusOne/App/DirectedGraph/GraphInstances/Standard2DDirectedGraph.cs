using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph.GraphInstances;

public class Standard2DDirectedGraph(IOptions<AppSettings> appSettings,
                                     IEnumerable<IDirectedGraphDrawingService> graphServices,
                                     ILightSourceService lightSourceService,
                                     IConsoleService consoleService,
                                     ShapeFactory shapeFactory)
                                        : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
                                          IDirectedGraph
{
    private readonly Dictionary<(int, int), List<(double X, double Y)>> _nodeGrid = [];

    private int _nodesPositioned = 0;

    public GraphType GraphType => GraphType.Standard2D;

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
        // Set up the base node's position
        (double X, double Y) baseNodePosition = (0, 0);

        _nodes[1].Position = baseNodePosition;
        _nodes[1].IsPositioned = true;
        _nodes[1].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

        _nodesPositioned = 1;

        //recursive method to position a node and its children
        PositionNode(_nodes[1]);

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
    private void PositionNode(DirectedGraphNode node)
    {
        if (!node.IsPositioned)
        {
            double nodeRadius = _appSettings.NodeAestheticSettings.NodeRadius;

            double xOffset = node.Parent == null
                                            ? 0
                                            : node.Parent.Position.X;

            double yOffset = node.Parent == null
                                            ? 0
                                            : node.Parent.Position.Y - _appSettings.NodeAestheticSettings.NodeSpacerY;

            node.Position = (xOffset, yOffset);

            if (_appSettings.NodeAestheticSettings.NodeRotationAngle != 0)
            {
                (double x, double y) = NodePositions.RotateNode(node.NumberValue,
                                                                _appSettings.NodeAestheticSettings.NodeRotationAngle,
                                                                xOffset,
                                                                yOffset);

                node.Position = (x, y);
            }

            double minDistance = nodeRadius * 2;

            while (NodeOverlapsNeighbours(node, minDistance))
            {
                // first move the node to the right
                node.Position = (node.Position.X + _appSettings.NodeAestheticSettings.NodeSpacerX, node.Position.Y);

                // now check if moving to the right caused node overlap
                if (NodeOverlapsNeighbours(node, minDistance))
                {
                    // move to the left instead
                    node.Position = (node.Position.X - 2 * _appSettings.NodeAestheticSettings.NodeSpacerX, node.Position.Y);
                }
            }

            AddNodeToGrid(node, minDistance);

            node.Shape.Radius = nodeRadius;
            node.IsPositioned = true;
            _nodesPositioned += 1;

            _consoleService.Write($"\r{_nodesPositioned} nodes positioned... ");
        }

        foreach (DirectedGraphNode childNode in node.Children)
        {
            PositionNode(childNode);
        }
    }

    /// <summary>
    /// Determine if the node that was just positioned is too close to neighbouring nodes (and thus overlapping).
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    private bool NodeOverlapsNeighbours(DirectedGraphNode newNode,
                                       double minDistance)
    {
        (int, int) cell = GetGridCellForNode(newNode, minDistance);

        // Check this cell and adjacent cells
        foreach ((int, int) offset in new[] { (0, 0), (1, 0), (0, 1), (-1, 0), (0, -1) })
        {
            (int, int) checkCell = (cell.Item1 + offset.Item1,
                                    cell.Item2 + offset.Item2);

            if (_nodeGrid.TryGetValue(checkCell, out var nodesInCell))
            {
                foreach ((double X, double Y) node in nodesInCell)
                {
                    if (Distance(newNode.Position, node) < minDistance)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
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