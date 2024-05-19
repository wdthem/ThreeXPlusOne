using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.Shapes;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public class TwoDimensionalDirectedGraph(IOptions<AppSettings> appSettings,
                                         IEnumerable<IDirectedGraphDrawingService> graphServices,
                                         ILightSourceService lightSourceService,
                                         IConsoleService consoleService,
                                         ShapeFactory shapeFactory)
                                                : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
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
        (double X, double Y) base1 = (0, 0);                                                             // Node '1' at the bottom
        (double X, double Y) base2 = (0, base1.Y - _appSettings.NodeAestheticSettings.NodeSpacerY);      // Node '2' just above '1'
        (double X, double Y) base4 = (0, base2.Y - _appSettings.NodeAestheticSettings.NodeSpacerY);      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].IsPositioned = true;
        _nodes[1].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

        _nodes[2].Position = base2;
        _nodes[2].IsPositioned = true;
        _nodes[2].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

        _nodes[4].Position = base4;
        _nodes[4].IsPositioned = true;
        _nodes[4].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;

        _nodesPositioned = 3;

        //recursive method to position a node and its children
        PositionNode(_nodes[1]);

        _consoleService.WriteDone();

        NodePositions.MoveNodesToPositiveCoordinates(_nodes,
                                                     _appSettings.NodeAestheticSettings.NodeSpacerX,
                                                     _appSettings.NodeAestheticSettings.NodeSpacerY,
                                                     _appSettings.NodeAestheticSettings.NodeRadius);
    }

    /// <summary>
    /// Set the shapes and colours of the positioned nodes
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
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        if (!node.IsPositioned)
        {
            double nodeRadius = _appSettings.NodeAestheticSettings.NodeRadius;

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

                    xOffset = xOffset - (allNodesAtDepth / 2 * _appSettings.NodeAestheticSettings.NodeSpacerX) + (_appSettings.NodeAestheticSettings.NodeSpacerX * addedWidth);
                }
            }

            double yOffset = node.Parent!.Position.Y - _appSettings.NodeAestheticSettings.NodeSpacerY;

            node.Position = (xOffset, yOffset);

            if (_appSettings.NodeAestheticSettings.NodeRotationAngle != 0)
            {
                (double x, double y) = NodePositions.RotateNode(node.NumberValue,
                                                                _appSettings.NodeAestheticSettings.NodeRotationAngle,
                                                                xOffset,
                                                                yOffset);

                node.Position = (x, y);
            }

            double signedXAxisDistanceFromParent = XAxisSignedDistanceFromParent(node.Position, node.Parent.Position);
            double absoluteXAxisDistanceFromParent = Math.Abs(signedXAxisDistanceFromParent);

            //limit the x-axis distance between node and parent, because the distance calculated above based on allNodesAtDepth can push
            //parents and children too far away from each other on the x-axis
            if (absoluteXAxisDistanceFromParent > _appSettings.NodeAestheticSettings.NodeSpacerX * 3)
            {
                //if the child node is to the left of the parent
                if (signedXAxisDistanceFromParent < 0)
                {
                    node.Position = (node.Position.X + ((absoluteXAxisDistanceFromParent / 3) - _appSettings.NodeAestheticSettings.NodeSpacerX), node.Position.Y);
                }
                else
                {
                    node.Position = (node.Position.X - ((absoluteXAxisDistanceFromParent / 3) + _appSettings.NodeAestheticSettings.NodeSpacerX), node.Position.Y);
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
        }

        foreach (DirectedGraphNode childNode in node.Children)
        {
            PositionNode(childNode);
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
