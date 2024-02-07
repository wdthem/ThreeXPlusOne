using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public class TwoDimensionalDirectedGraph(IOptions<AppSettings> appSettings,
                                         IEnumerable<IDirectedGraphService> graphServices,
                                         ILightSourceService lightSourceService,
                                         IConsoleService consoleService)
                                                : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService),
                                                  IDirectedGraph
{
    private int _nodesPositioned = 0;

    public int Dimensions => 2;

    /// <summary>
    /// Assign sizes to the canvas width and height after having positioned the nodes
    /// </summary>
    public void SetCanvasDimensions()
    {
        SetCanvasSize();
    }

    /// <summary>
    /// Generate a 2D visual representation of the directed graph
    /// </summary>
    public void Draw()
    {
        DrawDirectedGraph();
    }

    /// <summary>
    /// Position the nodes on the graph in 2D space
    /// </summary>
    public void PositionNodes()
    {
        // Set up the base nodes' positions
        (double X, double Y) base1 = (0, 0);                                    // Node '1' at the bottom
        (double X, double Y) base2 = (0, base1.Y - _appSettings.YNodeSpacer);      // Node '2' just above '1'
        (double X, double Y) base4 = (0, base2.Y - _appSettings.YNodeSpacer);      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].IsPositioned = true;
        _nodes[1].Shape.Radius = _appSettings.NodeRadius;

        _nodes[2].Position = base2;
        _nodes[2].IsPositioned = true;
        _nodes[2].Shape.Radius = _appSettings.NodeRadius;

        _nodes[4].Position = base4;
        _nodes[4].IsPositioned = true;
        _nodes[4].Shape.Radius = _appSettings.NodeRadius;

        List<DirectedGraphNode> nodesToDraw = _nodes.Values.Where(n => n.Depth == _nodes[4].Depth + 1)
                                                           .ToList();
        _nodesPositioned = 3;

        foreach (DirectedGraphNode node in nodesToDraw)
        {
            PositionNode(node);
        }

        _consoleService.WriteDone();

        _nodePositions.MoveNodesToPositiveCoordinates(_nodes,
                                                      _appSettings.XNodeSpacer,
                                                      _appSettings.YNodeSpacer,
                                                      _appSettings.NodeRadius);
    }

    /// <summary>
    /// Set the shapes of the positioned nodes
    /// </summary>
    public void SetNodeShapes()
    {
        foreach (DirectedGraphNode node in _nodes.Values.Where(node => node.IsPositioned))
        {
            NodeAesthetics.SetNodeShape(node,
                                        _appSettings.NodeRadius,
                                        _appSettings.IncludePolygonsAsNodes);
        }
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        double nodeRadius = _appSettings.NodeRadius;

        if (!node.IsPositioned)
        {
            int allNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth);

            int positionedNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth && n.IsPositioned);

            double xOffset = node.Parent == null
                                    ? 0
                                    : node.Parent.Position.X;

            if (allNodesAtDepth > 1)
            {
                if (node.Parent!.Children.Count == 1)
                {
                    xOffset = node.Parent.Position.X;
                }
                else
                {
                    int addedWidth;

                    if (allNodesAtDepth % 2 == 0)
                    {
                        addedWidth = positionedNodesAtDepth == 0
                                                            ? 0
                                                            : positionedNodesAtDepth + 1;
                    }
                    else
                    {
                        addedWidth = positionedNodesAtDepth == 0
                                                            ? 0
                                                            : positionedNodesAtDepth;
                    }

                    xOffset = xOffset - (allNodesAtDepth / 2 * _appSettings.XNodeSpacer) + (_appSettings.XNodeSpacer * addedWidth);
                }
            }

            double yOffset = node.Parent!.Position.Y - _appSettings.YNodeSpacer;

            node.Position = (xOffset, yOffset);

            if (_appSettings.NodeRotationAngle != 0)
            {
                (double x, double y) = NodePositions.RotateNode(node.NumberValue,
                                                                _appSettings.NodeRotationAngle,
                                                                xOffset,
                                                                yOffset);

                node.Position = (x, y);
            }

            double signedXAxisDistanceFromParent = XAxisSignedDistanceFromParent(node.Position, node.Parent.Position);
            double absoluteXAxisDistanceFromParent = Math.Abs(signedXAxisDistanceFromParent);

            //limit the x-axis distance between node and parent, because the distance calculated above based on allNodesAtDepth can push
            //parents and children too far away from each other on the x-axis
            if (absoluteXAxisDistanceFromParent > _appSettings.XNodeSpacer * 3)
            {
                //if the child node is to the left of the parent
                if (signedXAxisDistanceFromParent < 0)
                {
                    node.Position = (node.Position.X + ((absoluteXAxisDistanceFromParent / 3) - _appSettings.XNodeSpacer), node.Position.Y);
                }
                else
                {
                    node.Position = (node.Position.X - ((absoluteXAxisDistanceFromParent / 3) + _appSettings.XNodeSpacer), node.Position.Y);
                }
            }

            double minDistance = nodeRadius * 2;

            while (_nodePositions.NodeOverlapsNeighbours(node, minDistance))
            {
                node.Position = (node.Position.X + (node.IsFirstChild
                                                                ? -nodeRadius * 2 - 40
                                                                : nodeRadius * 2 + 40),
                                 node.Position.Y);
            }

            _nodePositions.AddNodeToGrid(node, minDistance);

            node.Shape.Radius = nodeRadius;
            node.IsPositioned = true;
            _nodesPositioned += 1;

            _consoleService.Write($"\r{_nodesPositioned} nodes positioned... ");

            foreach (DirectedGraphNode childNode in node.Children)
            {
                PositionNode(childNode);
            }
        }
    }

    /// <summary>
    /// Calculate the signed X-axis distance from a parent to a child node
    /// </summary>
    /// <remarks>
    /// Negative if child is the to left of the parent, positive if to the right
    /// </remarks>
    /// <param name="childPosition"></param>
    /// <param name="parentPosition"></param>
    /// <returns></returns>
    private static double XAxisSignedDistanceFromParent((double X, double Y) childPosition,
                                                        (double X, double Y) parentPosition)
    {
        return childPosition.X - parentPosition.X;
    }
}