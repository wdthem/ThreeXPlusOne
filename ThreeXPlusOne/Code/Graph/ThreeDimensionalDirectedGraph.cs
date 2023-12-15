using System.Xml.Linq;
using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public class ThreeDimensionalDirectedGraph : DirectedGraph, IDirectedGraph
{
    private readonly IOptions<Settings> _settings;
    private readonly IFileHelper _fileHelper;

    public int Dimensions => 3;

    public ThreeDimensionalDirectedGraph(IOptions<Settings> settings,
                                         IFileHelper fileHelper)
    {
        _settings = settings;
        _fileHelper = fileHelper;
    }

    public void PositionNodes()
    {
        Console.Write("Positioning nodes... ");

        // Set up the base nodes' positions
        var base1 = new SKPoint(_settings.Value.CanvasWidth / 2, _settings.Value.CanvasHeight - 100);         // Node '1' at the bottom
        var base2 = new SKPoint(_settings.Value.CanvasWidth / 2, base1.Y - _settings.Value.YNodeSpacer);      // Node '2' just above '1'
        var base4 = new SKPoint(_settings.Value.CanvasWidth / 2, base2.Y - _settings.Value.YNodeSpacer);      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].Z = 0;
        _nodes[1].Radius = _settings.Value.NodeRadius;
        _nodes[1].IsPositioned = true;

        _nodes[2].Position = base2;
        _nodes[2].Z = 0;
        _nodes[2].Radius = _settings.Value.NodeRadius;
        _nodes[2].IsPositioned = true;

        _nodes[4].Position = base4;
        _nodes[4].Z = 0;
        _nodes[4].Radius = _settings.Value.NodeRadius;
        _nodes[4].IsPositioned = true;

        List<DirectedGraphNode> nodesToDraw = _nodes.Where(n => n.Value.Depth == _nodes[4].Depth + 1)
                                                    .Select(n => n.Value)
                                                    .ToList();

        foreach (var node in nodesToDraw)
        {
            PositionNode(node);
        }

        // Assuming nodeList is your initial list of nodes
        var allNodes = nodesToDraw.SelectMany(FlattenHierarchy).ToList();

        var nodesWithSamePosition = allNodes.GroupBy(node => node.Position)
                                            .Where(group => group.Count() > 1)
                                            .SelectMany(group => group)
                                            .Distinct() // To remove duplicates if a node appears in multiple hierarchies
                                            .ToList();

        //Instead of random x,y movement, do the angle rotation to a random angle
        foreach (var node in nodesWithSamePosition)
        {
            var randomX = _random.Next(1, _settings.Value.XNodeSpacer / 2);
            var randomY = _random.Next(1, _settings.Value.YNodeSpacer / 2);

            node.Position = new SKPoint(node.Position.X + randomX, node.Parent!.Position.Y - randomY);
        }

        ConsoleOutput.WriteDone();
    }

    private static IEnumerable<DirectedGraphNode> FlattenHierarchy(DirectedGraphNode node)
    {
        yield return node;

        foreach (var child in node.Children)
        {
            foreach (var childNode in FlattenHierarchy(child))
            {
                yield return childNode;
            }
        }
    }

    public void Draw(Settings settings)
    {
        Console.WriteLine("Drawing connections and nodes... ");

        using var surface = SKSurface.Create(new SKImageInfo(_settings.Value.CanvasWidth, _settings.Value.CanvasHeight));

        SKCanvas canvas = surface.Canvas;

        canvas.Clear(SKColors.Black);

        var lcv = 1;
        foreach (var node in _nodes)
        {
            DrawConnection(canvas, node.Value);

            Console.Write($"    \r{lcv} connections drawn");

            lcv += node.Value.Children.Count;
        }

        Console.WriteLine();

        lcv = 1;
        foreach (var node in _nodes)
        {
            DrawNode(canvas, node.Value);

            Console.Write($"    \r{lcv} nodes drawn");

            lcv = lcv + 1 + node.Value.Children.Count;
        }

        Console.WriteLine();
        ConsoleOutput.WriteDone();

        if (settings.GenerateGraph)
        {
            string fullPath = _fileHelper.GenerateGraphFilePath();

            if (string.IsNullOrEmpty(fullPath))
            {
                ConsoleOutput.WriteError("Invalid ImagePath. Check 'settings.json'");

                return;
            }

            Console.WriteLine();
            Console.Write("Generate 3D visualization? (y/n): ");
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.Key != ConsoleKey.Y)
            {
                Console.WriteLine("");
                Console.WriteLine("Image generation cancelled");
                Console.WriteLine("");

                return;
            }

            Console.WriteLine();

            SaveCanvas(surface, fullPath);
        }
        else
        {
            Console.WriteLine("Graph generation disabled");
        }
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
        float depthFactor = 1 - (node.Z / maxZ);
        float nodeRadius = baseRadius * (0.5f + depthFactor * 0.5f);

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
                nodeRadius = node.Parent.Radius * (0.75f + depthFactor * 0.99f); // Larger for closer (left) nodes
            }
            else
            {
                xOffset = (xOffset + ((allNodesAtDepth / 2) * _settings.Value.XNodeSpacer)) + (_settings.Value.XNodeSpacer * addedWidth);
                node.Z += 10;
                nodeRadius = node.Parent.Radius * (0.5f + depthFactor * 0.98f); // Smaller for further (right) nodes
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

    public SKPoint ApplyPerspectiveTransform(DirectedGraphNode node, float d)
    {

        float xCentered = node.Position.X - _settings.Value.CanvasWidth / 2;
        float yCentered = node.Position.Y - _settings.Value.CanvasHeight / 2;
        float xPrime = xCentered / (1 + node.Z / d) + _settings.Value.CanvasWidth / 2;
        float yPrime = yCentered / (1 + node.Z / d) + _settings.Value.CanvasHeight / 2;

        //float xPrime = node.Position.X / (1 + reversedDepth / d);
        //float yPrime = node.Position.Y / (1 + reversedDepth / d);

        return new SKPoint(xPrime, yPrime);
    }

    private static void DrawConnection(SKCanvas canvas, DirectedGraphNode node)
    {
        var paint = new SKPaint
        {
            Color = new SKColor(255, 255, 255, 128),
            StrokeWidth = 2,
            IsAntialias = true
        };

        foreach (var childNode in node.Children)
        {
            canvas.DrawLine(node.Position, childNode.Position, paint);
        }
    }

    private void DrawNode(SKCanvas canvas, DirectedGraphNode node)
    {
        var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = GetRandomColor(128)
        };

        var textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            TextSize = 20,
            FakeBoldText = true,
        };

        if (_settings.Value.DistortNodes)
        {
            DrawDistortedPath(canvas,
                              node.Position,
                              _settings.Value.NodeRadius,
                              _settings.Value.RadiusDistortion,
                              paint);
        }
        else
        {
            canvas.DrawCircle(node.Position,
                              node.Radius,
                              paint);
        }

        // Draw the text
        // Adjust the Y coordinate to account for text height (this centers the text vertically in the circle)
        float textY = node.Position.Y + 8;

        canvas.DrawText(node.Value.ToString(), node.Position.X, textY, textPaint);
    }

    private void DrawDistortedPath(SKCanvas canvas,
                                   SKPoint center,
                                   float baseRadius,
                                   int distortionLevel,
                                   SKPaint paint)
    {
        var path = new SKPath();
        var randomPointsCount = _random.Next(1, 9); //from 1 to 8

        path.MoveTo(center.X + baseRadius, center.Y);

        for (int i = 1; i <= randomPointsCount; i++)
        {
            float angle = (float)(2 * Math.PI / randomPointsCount * i);
            float radiusVariation = _random.Next(4, distortionLevel) + 1;
            float radius = baseRadius + radiusVariation;

            var point = new SKPoint(
                center.X + radius * (float)Math.Cos(angle),
                center.Y + radius * (float)Math.Sin(angle));

            path.LineTo(point);
        }

        path.Close();

        canvas.DrawPath(path, paint);
    }
}