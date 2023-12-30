using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public class ThreeDimensionalDirectedGraph(IOptions<Settings> settings,
                                           IFileHelper fileHelper,
                                           IConsoleHelper consoleHelper) : DirectedGraph(settings, fileHelper, consoleHelper), IDirectedGraph
{
    public int Dimensions => 3;
    private int _nodesPositioned = 0;

    /// <summary>
    /// Generate a 3D visual representation of the directed graph
    /// </summary>
    public void Draw()
    {
        DrawGraph();
    }

    /// <summary>
    /// Position the nodes on the graph in pseudo-3D space
    /// </summary>
    public void PositionNodes()
    {
        // Set up the base nodes' positions
        var base1 = new SKPoint(0, 100);         // Node '1' at the bottom
        var base2 = new SKPoint(0, base1.Y - (_settings.YNodeSpacer * 5));      // Node '2' just above '1'
        var base4 = new SKPoint(0, base2.Y - (_settings.YNodeSpacer * 5));      // Node '4' above '2'

        _nodes[1].Position = base1;
        //_nodes[1].Position = ApplyPerspectiveTransform(_nodes[1], _settings.DistanceFromViewer);
        _nodes[1].Radius = _settings.NodeRadius;
        _nodes[1].IsPositioned = true;

        _nodes[2].Position = base2;
        //_nodes[2].Position = ApplyPerspectiveTransform(_nodes[2], _settings.DistanceFromViewer);
        _nodes[2].Radius = _settings.NodeRadius;
        _nodes[2].IsPositioned = true;

        _nodes[4].Position = base4;
        //_nodes[4].Position = ApplyPerspectiveTransform(_nodes[4], _settings.DistanceFromViewer);
        _nodes[4].Radius = _settings.NodeRadius;
        _nodes[4].IsPositioned = true;

        List<DirectedGraphNode> nodesToDraw = _nodes.Where(n => n.Value.Depth == _nodes[4].Depth + 1)
                                                    .Select(n => n.Value)
                                                    .ToList();

        _nodesPositioned = 3;

        foreach (var node in nodesToDraw)
        {
            PositionNode(node);
        }

        AdjustNodesWithSamePosition(nodesToDraw);

        _consoleHelper.WriteDone();
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        if (node.IsPositioned)
        {
            return;
        }

        int allNodesAtDepth =
            _nodes.Values.Count(n => n.Depth == node.Depth);

        int positionedNodesAtDepth =
            _nodes.Values.Count(n => n.Depth == node.Depth && n.IsPositioned);

        var baseRadius = _settings.NodeRadius;

        if (node.Parent != null && node.Parent.Radius > 0)
        {
            baseRadius = node.Parent.Radius;
        }

        float maxZ = _nodes.Max(node => node.Value.Z);
        float depthFactor = node.Z / maxZ;
        float scale = 0.98f - depthFactor * 0.1f;
        float minScale = (float)0.3;
        float nodeRadius = baseRadius * Math.Max(scale - 0.02f, minScale);

        float xOffset = node.Parent == null
                                ? 0
                                : node.Parent.Position.X;


        if (node.Parent!.Children.Count == 1)
        {
            xOffset = node.Parent.Position.X;
            nodeRadius = node.Parent.Radius;
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

            if (node.IsFirstChild)
            {
                xOffset = (xOffset - ((allNodesAtDepth / 2) * _settings.XNodeSpacer)) - (_settings.XNodeSpacer * addedWidth);
                node.Z -= 25;
                nodeRadius = node.Parent.Radius * Math.Max(scale, minScale);
            }
            else
            {
                xOffset = (xOffset + ((allNodesAtDepth / 2) * _settings.XNodeSpacer)) + (_settings.XNodeSpacer * addedWidth);
                node.Z += 10;
                nodeRadius = node.Parent.Radius * Math.Max(scale - 0.02f, minScale);
            }
        }

        var yOffset = node.Parent!.Position.Y - _settings.YNodeSpacer;

        node.Radius = nodeRadius;
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

        if (node.Parent != null && node.Parent.Children.Count == 2)
        {
            node.Position = ApplyPerspectiveTransform(node, _settings.DistanceFromViewer);
        }

        node.IsPositioned = true;
        _nodesPositioned += 1;

        _consoleHelper.Write($"\r{_nodesPositioned} nodes positioned... ");

        foreach (var childNode in node.Children)
        {
            PositionNode(childNode);
        }
    }

    /// <summary>
    /// For the pseudo-three-dimensional graph, apply depth to the given node based on the Z coordinate.
    /// The Z-coordinate is set to the reverse of the depth value of the node in the AddSeries() method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="d">The distance to the viewer</param>
    /// <returns></returns>
    private SKPoint ApplyPerspectiveTransform(DirectedGraphNode node, float d)
    {
        float xCentered = node.Position.X;
        float yCentered = node.Position.Y;

        float xPrime = xCentered / (1 + node.Z / d) + 0;
        float yPrime = yCentered / (1 + node.Z / d) - (_settings.YNodeSpacer * 3);

        return new SKPoint(xPrime, yPrime);
    }
}