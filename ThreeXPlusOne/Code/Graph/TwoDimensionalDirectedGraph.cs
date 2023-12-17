using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public class TwoDimensionalDirectedGraph : DirectedGraph, IDirectedGraph
{
    public int Dimensions => 2;

    public TwoDimensionalDirectedGraph(IOptions<Settings> settings,
                                       IFileHelper fileHelper) : base(settings, fileHelper)
    {
    }

    /// <summary>
    /// Generate a 2D visual representation of the directed graph
    /// </summary>
    public void DrawGraph()
    {
        Draw();
    }

    /// <summary>
    /// Position the nodes on the graph in 2D space
    /// </summary>
    public void PositionNodes()
    {
        Console.Write("Positioning nodes... ");

        // Set up the base nodes' positions
        var base1 = new SKPoint(_settings.Value.CanvasWidth / 2, _settings.Value.CanvasHeight - 100);         // Node '1' at the bottom
        var base2 = new SKPoint(_settings.Value.CanvasWidth / 2, base1.Y - _settings.Value.YNodeSpacer);      // Node '2' just above '1'
        var base4 = new SKPoint(_settings.Value.CanvasWidth / 2, base2.Y - _settings.Value.YNodeSpacer);      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].IsPositioned = true;

        _nodes[2].Position = base2;
        _nodes[2].IsPositioned = true;

        _nodes[4].Position = base4;
        _nodes[4].IsPositioned = true;

        List<DirectedGraphNode> nodesToDraw = _nodes.Where(n => n.Value.Depth == _nodes[4].Depth + 1)
                                                    .Select(n => n.Value)
                                                    .ToList();

        foreach (var node in nodesToDraw)
        {
            PositionNode(node);
        }

        AdjustNodesWithSamePosition(nodesToDraw);

        ConsoleOutput.WriteDone();
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        node.Radius = _settings.Value.NodeRadius;

        if (!node.IsPositioned)
        {
            int allNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth);

            int positionedNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth && n.IsPositioned);

            float xOffset = node.Parent == null
                                    ? _settings.Value.CanvasWidth / 2
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

                    xOffset = (xOffset - ((allNodesAtDepth / 2) * _settings.Value.XNodeSpacer)) + (_settings.Value.XNodeSpacer * addedWidth);
                }
            }

            var yOffset = node.Parent!.Position.Y - _settings.Value.YNodeSpacer;

            node.Position = new SKPoint(xOffset, yOffset);

            if (_settings.Value.NodeRotationAngle != 0)
            {
                (double x, double y) rotatedPosition;

                if (node.Value % 2 == 0)
                {
                    rotatedPosition = RotatePointClockwise(xOffset, yOffset, _settings.Value.NodeRotationAngle);
                }
                else
                {
                    rotatedPosition = RotatePointAntiClockWise(xOffset, yOffset, _settings.Value.NodeRotationAngle);
                }

                node.Position = new SKPoint((float)rotatedPosition.x, (float)rotatedPosition.y);
            }

            node.IsPositioned = true;

            foreach (var childNode in node.Children)
            {
                PositionNode(childNode);
            }
        }
    }
}