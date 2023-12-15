using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public abstract class DirectedGraph
{
    protected readonly Dictionary<int, DirectedGraphNode> _nodes;
    protected readonly Random _random;
    protected readonly IOptions<Settings> _settings;
    protected readonly IFileHelper _fileHelper;

    public DirectedGraph(IOptions<Settings> settings,
                         IFileHelper fileHelper)
	{
        _nodes = new Dictionary<int, DirectedGraphNode>();
        _random = new Random();
        _settings = settings;
        _fileHelper = fileHelper;
    }

    public void AddSeries(List<int> series)
    {
        DirectedGraphNode? previousNode = null;
        int currentDepth = series.Count;

        foreach (var number in series)
        {
            if (!_nodes.TryGetValue(number, out DirectedGraphNode? currentNode))
            {
                currentNode = new DirectedGraphNode(number)
                {
                    Depth = currentDepth // Set the initial depth for the node
                };

                _nodes.Add(number, currentNode);
            }

            // Check if this is a deeper path to the current node
            if (currentDepth < currentNode.Depth)
            {
                currentNode.Depth = currentDepth;
            }

            if (previousNode != null)
            {
                previousNode.Parent = currentNode;

                // Check if previousNode is already a child to prevent duplicate additions
                if (!currentNode.Children.Contains(previousNode))
                {
                    previousNode.IsFirstChild = true;

                    if (currentNode.Children.Count == 1)
                    {
                        previousNode.IsFirstChild = false;
                        previousNode.IsSecondChild = true;
                    }
                    currentNode.Children.Add(previousNode);
                }
            }

            previousNode = currentNode;

            currentDepth--;  // decrement the depth as we move through the series
        }

        var maxNodeDepth = _nodes.Max(node => node.Value.Depth);


        foreach (var node in _nodes)
        {
            float z = maxNodeDepth - node.Value.Depth;

            node.Value.Z = z;
        }
    }

    protected static void SaveCanvas(SKSurface surface, string path)
    {
        Console.WriteLine();
        Console.Write("Saving image... ");

        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 25))
        using (var stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Saved to: {path}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    protected void MoveNodesWithSamePosition(List<DirectedGraphNode> nodes)
    {
        var allNodes = nodes.SelectMany(FlattenHierarchy).ToList();

        var nodesWithSamePosition = allNodes.GroupBy(node => node.Position)
                                            .Where(group => group.Count() > 1)
                                            .SelectMany(group => group)
                                            .Distinct() // To remove duplicates if a node appears in multiple hierarchies
                                            .ToList();

        //TODO: Instead of random x,y movement, do the angle rotation to a random angle
        foreach (var node in nodesWithSamePosition)
        {
            var randomX = _random.Next(1, _settings.Value.XNodeSpacer / 2);
            var randomY = _random.Next(1, _settings.Value.YNodeSpacer / 2);

            node.Position = new SKPoint(node.Position.X + randomX, node.Parent!.Position.Y - randomY);
        }
    }

    protected static IEnumerable<DirectedGraphNode> FlattenHierarchy(DirectedGraphNode node)
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

    protected SKColor GetRandomColor(byte alpha = 255)
    {
        byte red, green, blue;

        do
        {
            red = (byte)_random.Next(256);
            green = (byte)_random.Next(256);
            blue = (byte)_random.Next(256);
        }
        while (red == 0 && green == 0 && blue == 0); // Repeat if the color is black

        return new SKColor(red, green, blue, alpha);
    }

    protected static (double x, double y) RotatePointAntiClockWise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x - sinTheta * y;
        double yNew = sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }

    protected static (double x, double y) RotatePointClockwise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x + sinTheta * y;
        double yNew = -sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }

    protected void Draw()
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

        if (_settings.Value.GenerateGraph)
        {
            string fullPath = _fileHelper.GenerateGraphFilePath();

            if (string.IsNullOrEmpty(fullPath))
            {
                ConsoleOutput.WriteError($"Invalid {nameof(_settings.Value.OutputPath)}. Check 'settings.json'");

                return;
            }

            Console.WriteLine();
            Console.Write($"Generate {_settings.Value.GraphDimensions}D visualization? (y/n): ");
            ConsoleKeyInfo keyInfo = Console.ReadKey();

            if (keyInfo.Key != ConsoleKey.Y)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine("Graph generation cancelled");
                Console.WriteLine();

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
                              node.Radius,
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