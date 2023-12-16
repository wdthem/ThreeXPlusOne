using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public class ThreeDimensionalDirectedGraph : DirectedGraph, IDirectedGraph
{
    public int Dimensions => 3;

    public ThreeDimensionalDirectedGraph(IOptions<Settings> settings,
                                         IFileHelper fileHelper) : base(settings, fileHelper)
    {
    }

    /// <summary>
    /// Generate a 3D visual representation of the directed graph
    /// </summary>
    public void DrawGraph()
    {
        Draw();
    }

    /// <summary>
    /// Position the nodes on the graph in pseudo-3D space
    /// </summary>
    public void PositionNodes()
    {
        Console.Write("Positioning nodes... ");

        // Set up the base nodes' positions
        var base1 = new SKPoint(_settings.Value.CanvasWidth / 2, _settings.Value.CanvasHeight - 100);         // Node '1' at the bottom
        var base2 = new SKPoint(_settings.Value.CanvasWidth / 2, base1.Y - (_settings.Value.YNodeSpacer * 2));      // Node '2' just above '1'
        var base4 = new SKPoint(_settings.Value.CanvasWidth / 2, base2.Y - (_settings.Value.YNodeSpacer * 2));      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].Position = ApplyPerspectiveTransform(_nodes[1], (float)200);
        _nodes[1].Radius = _settings.Value.NodeRadius;
        _nodes[1].IsPositioned = true;

        _nodes[2].Position = base2;
        _nodes[2].Position = ApplyPerspectiveTransform(_nodes[2], (float)200);
        _nodes[2].Radius = _settings.Value.NodeRadius;
        _nodes[2].IsPositioned = true;

        _nodes[4].Position = base4;
        _nodes[4].Position = ApplyPerspectiveTransform(_nodes[4], (float)200);
        _nodes[4].Radius = _settings.Value.NodeRadius;
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

        var baseRadius = _settings.Value.NodeRadius;

        if (node.Parent != null &&  node.Parent.Radius > 0)
        {
            baseRadius = node.Parent.Radius;
        }

        float maxZ = _nodes.Max(node => node.Value.Z);
        float depthFactor = (node.Z / maxZ);
        float scale = 0.98f - depthFactor * 0.1f;
        float minScale = (float)0.3;
        float nodeRadius = baseRadius * Math.Max(scale - 0.02f, minScale);

        float xOffset = node.Parent == null
                                ? _settings.Value.CanvasWidth / 2
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
                xOffset = (xOffset - ((allNodesAtDepth / 2) * _settings.Value.XNodeSpacer)) - (_settings.Value.XNodeSpacer * addedWidth);
                node.Z -= 25;
                nodeRadius = node.Parent.Radius * Math.Max(scale, minScale);
            }
            else
            {
                xOffset = (xOffset + ((allNodesAtDepth / 2) * _settings.Value.XNodeSpacer)) + (_settings.Value.XNodeSpacer * addedWidth);
                node.Z += 10;
                nodeRadius = node.Parent.Radius * Math.Max(scale - 0.02f, minScale);
            }
        }
        
        var yOffset = node.Parent!.Position.Y - _settings.Value.YNodeSpacer;

        node.Radius = nodeRadius;
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

        if (node.Parent != null && node.Parent.Children.Count == 2)
        {
            node.Position = ApplyPerspectiveTransform(node, (float)200);
        }
        
        node.IsPositioned = true;

        foreach (var childNode in node.Children)
        {
            PositionNode(childNode);
        }
    }

    private SKPoint ApplyPerspectiveTransform(DirectedGraphNode node, float d)
    {
        float xCentered = node.Position.X - _settings.Value.CanvasWidth / 2;
        float yCentered = node.Position.Y - _settings.Value.CanvasHeight / 2;

        float xPrime = xCentered / (1 + node.Z / d) + _settings.Value.CanvasWidth / 2;
        float yPrime = yCentered / (1 + node.Z / d) + _settings.Value.CanvasHeight / 2;

        return new SKPoint(xPrime, yPrime);
    }
}