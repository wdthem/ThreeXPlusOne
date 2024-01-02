using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public class TwoDimensionalDirectedGraph(IOptions<Settings> settings,
                                         IFileHelper fileHelper,
                                         IConsoleHelper consoleHelper) : DirectedGraph(settings, fileHelper, consoleHelper), IDirectedGraph
{
    public int Dimensions => 2;
    private int _nodesPositioned = 0;

    /// <summary>
    /// Generate a 2D visual representation of the directed graph
    /// </summary>
    public void Draw()
    {
        DrawGraph();
    }

    /// <summary>
    /// Position the nodes on the graph in 2D space
    /// </summary>
    public void PositionNodes()
    {
        // Set up the base nodes' positions
        var base1 = new SKPoint(0, 0);                                    // Node '1' at the bottom
        var base2 = new SKPoint(0, base1.Y - _settings.YNodeSpacer);      // Node '2' just above '1'
        var base4 = new SKPoint(0, base2.Y - _settings.YNodeSpacer);      // Node '4' above '2'

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
                        addedWidth = positionedNodesAtDepth == 0 ? 0 : positionedNodesAtDepth + 1;
                    }
                    else
                    {
                        addedWidth = positionedNodesAtDepth == 0 ? 0 : positionedNodesAtDepth;
                    }

                    xOffset = (xOffset - ((allNodesAtDepth / 2) * _settings.XNodeSpacer)) + (_settings.XNodeSpacer * addedWidth);
                }
            }

            var yOffset = node.Parent!.Position.Y - _settings.YNodeSpacer;

            node.Position = new SKPoint(xOffset, yOffset);

            if (_settings.NodeRotationAngle != 0)
            {
                (double x, double y) rotatedPosition;

                if (node.Value % 2 == 0)
                {
                    rotatedPosition = RotatePointAntiClockwise(xOffset, yOffset, _settings.NodeRotationAngle);
                }
                else
                {
                    rotatedPosition = RotatePointClockwise(xOffset, yOffset, _settings.NodeRotationAngle);
                }

                node.Position = new SKPoint((float)rotatedPosition.x, (float)rotatedPosition.y);
            }

            float minDistance = _settings.NodeRadius * 2;

            while (NodeIsTooCloseToNeighbours(node, minDistance))
            {
                node.Position = new SKPoint((float)node.Position.X + (node.IsFirstChild
                                                                        ? -_settings.NodeRadius * 2 - 40
                                                                        : _settings.NodeRadius * 2 + 40),
                                            (float)node.Position.Y);
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