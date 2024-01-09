using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public class TwoDimensionalDirectedGraph(IOptions<Settings> settings,
                                         IEnumerable<IGraphService> graphServices,
                                         IFileHelper fileHelper,
                                         IConsoleHelper consoleHelper) : DirectedGraph(settings, graphServices, fileHelper, consoleHelper), IDirectedGraph
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
        (float X, float Y) base1 = (0, 0);                                    // Node '1' at the bottom
        (float X, float Y) base2 = (0, base1.Y - _settings.YNodeSpacer);      // Node '2' just above '1'
        (float X, float Y) base4 = (0, base2.Y - _settings.YNodeSpacer);      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].IsPositioned = true;
        _nodes[1].Radius = _settings.NodeRadius;

        _nodes[2].Position = base2;
        _nodes[2].IsPositioned = true;
        _nodes[2].Radius = _settings.NodeRadius;

        _nodes[4].Position = base4;
        _nodes[4].IsPositioned = true;
        _nodes[4].Radius = _settings.NodeRadius;

        List<DirectedGraphNode> nodesToDraw = _nodes.Where(n => n.Value.Depth == _nodes[4].Depth + 1)
                                                    .Select(n => n.Value)
                                                    .ToList();
        _nodesPositioned = 3;

        foreach (var node in nodesToDraw)
        {
            PositionNode(node);
        }

        _consoleHelper.WriteDone();

        MoveNodesToPositiveCoordinates();
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        node.Radius = _settings.NodeRadius;

        if (!node.IsPositioned)
        {
            int allNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth);

            int positionedNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth && n.IsPositioned);

            float xOffset = node.Parent == null
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

                    xOffset = xOffset - (allNodesAtDepth / 2 * _settings.XNodeSpacer) + (_settings.XNodeSpacer * addedWidth);
                }
            }

            float yOffset = node.Parent!.Position.Y - _settings.YNodeSpacer;

            node.Position = (xOffset, yOffset);

            if (_settings.NodeRotationAngle != 0)
            {
                (float x, float y) = RotateNode(node.Value, xOffset, yOffset);

                node.Position = (x, y);
            }

            float signedXAxisDistanceFromParent = XAxisSignedDistanceFromParent(node.Position, node.Parent.Position);
            float absoluteXAxisDistanceFromParent = Math.Abs(signedXAxisDistanceFromParent);

            //limit the x-axis distance between node and parent, because the distance calculated above based on allNodesAtDepth can push
            //parents and children too far away from each other on the x-axis
            if (absoluteXAxisDistanceFromParent > _settings.XNodeSpacer * 3)
            {
                //if the child node is to the left of the parent
                if (signedXAxisDistanceFromParent < 0)
                {
                    node.Position = (node.Position.X + ((absoluteXAxisDistanceFromParent / 3) - _settings.XNodeSpacer), node.Position.Y);
                }
                else
                {
                    node.Position = (node.Position.X - ((absoluteXAxisDistanceFromParent / 3) + _settings.XNodeSpacer), node.Position.Y);
                }
            }

            float minDistance = _settings.NodeRadius * 2;

            while (NodeIsTooCloseToNeighbours(node, minDistance))
            {
                node.Position = (node.Position.X + (node.IsFirstChild
                                                                ? -_settings.NodeRadius * 2 - 40
                                                                : _settings.NodeRadius * 2 + 40),
                                 node.Position.Y);
            }

            AddNodeToGrid(node, minDistance);

            node.IsPositioned = true;
            _nodesPositioned += 1;

            _consoleHelper.Write($"\r{_nodesPositioned} nodes positioned... ");

            foreach (var childNode in node.Children)
            {
                PositionNode(childNode);
            }
        }
    }
}