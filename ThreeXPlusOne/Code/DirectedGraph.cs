using SkiaSharp;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

internal class DirectedGraphNode
{
    public DirectedGraphNode(int value)
    {
        Value = value;
        Children = new List<DirectedGraphNode>();
        Depth = -1;
    }

    public int Value { get; set; }
    public DirectedGraphNode? Parent { get; set; }
    public List<DirectedGraphNode> Children { get; set; }
    public int Depth { get; set; }
    public SKPoint Position { get; set; }
    public bool IsPositioned { get; set; }
}

internal class DirectedGraph
{
    private Dictionary<int, DirectedGraphNode> _nodes;
    private readonly Random _random;
    private readonly Settings _settings;

    internal DirectedGraph(Settings settings)
    {
        _nodes = new Dictionary<int, DirectedGraphNode>();
        _random = new Random();
        _settings = settings;
    }

    public void AddSeries(IEnumerable<int> series)
    {
        DirectedGraphNode? previousNode = null;
        int currentDepth = series.Count();

        foreach (var number in series)
        {
            DirectedGraphNode? currentNode;
            if (!_nodes.TryGetValue(number, out currentNode))
            {
                currentNode = new DirectedGraphNode(number);
                currentNode.Depth = currentDepth; // Set the initial depth for the node
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
                    currentNode.Children.Add(previousNode);
                }
            }

            previousNode = currentNode;

            currentDepth--;  // decrement the depth as we move through the series
        }
    }

    public void PositionNodes()
    {
        Console.WriteLine("Positioning nodes...");

        // Set up the base nodes' positions
        SKPoint base1 = new SKPoint(_settings.CanvasWidth / 2, _settings.CanvasHeight - 100);         // Node '1' at the bottom
        SKPoint base2 = new SKPoint(_settings.CanvasWidth / 2, base1.Y - _settings.YNodeSpacer);      // Node '2' just above '1'
        SKPoint base4 = new SKPoint(_settings.CanvasWidth / 2, base2.Y - _settings.YNodeSpacer);      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].IsPositioned = true;

        _nodes[2].Position = base2;
        _nodes[2].IsPositioned = true;

        _nodes[4].Position = base4;
        _nodes[4].IsPositioned = true;

        List<DirectedGraphNode> nodesToDraw = _nodes.Where(n => n.Value.Depth == _nodes[4].Depth + 1)
                                                    .Select(n => n.Value)
                                                    .ToList();

        foreach (var node in nodesToDraw.Where(n => n.Value > 0))
        {
            PositionNode(node);
        }

        var allNodesWithParent = nodesToDraw.Where(n => n.Value > 0).SelectMany(node => FlattenHierarchy(node)).ToList();

        var nodesWithSamePosition = allNodesWithParent
            .GroupBy(item => item.Node.Position)
            .Where(group => group.Count() > 1)
            .Select(group => group
                // Order by depth and select the first node in the hierarchy
                .OrderBy(item => item.Node.Depth)
                .ThenBy(item => item.Parent != null ? item.Parent.Value : int.MinValue)
                .Select(item => item.Node)
                .First())
            .ToList();

        foreach (var node in nodesWithSamePosition)
        {
            node.Position = new SKPoint(node.Position.X + (_settings.XNodeSpacer * 2), node.Parent!.Position.Y - _settings.YNodeSpacer);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Done");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private IEnumerable<(DirectedGraphNode Node, DirectedGraphNode Parent)> FlattenHierarchy(DirectedGraphNode node, DirectedGraphNode? parent = null)
    {
        yield return (node!, parent!);

        foreach (var child in node.Children)
        {
            foreach (var childNode in FlattenHierarchy(child, node))
            {
                yield return childNode;
            }
        }
    }

    public void Draw(bool saveOutput, string outputFileName)
    {
        Console.WriteLine("Drawing nodes...");

        using (var surface = SKSurface.Create(new SKImageInfo(_settings.CanvasWidth, _settings.CanvasHeight)))
        {
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Black);

            foreach (var node in _nodes)
            {
                DrawConnection(canvas, node.Value);
            }

            foreach (var node in _nodes)
            {
                DrawNode(canvas, node.Value);
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Done");
            Console.ForegroundColor = ConsoleColor.White;

            if (saveOutput)
            {
                if (string.IsNullOrEmpty(outputFileName))
                {
                    throw new FileNotFoundException("The output file path was empty");
                }
                else
                {
                    var directory = Path.GetDirectoryName(outputFileName);

                    if (!Directory.Exists(directory))
                    {
                        throw new FileNotFoundException("The specified output directory does not exist");
                    }
                }

                SaveCanvas(surface, outputFileName);
            }
            else
            {
                Console.WriteLine("Output saving disabled");
            }
        }
    }

    private void PositionNode(DirectedGraphNode node)
    {
        if (!node.IsPositioned)
        {
            int allNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth);

            int positionedNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth && n.IsPositioned);

            float xOffset = node.Parent == null
                                    ? _settings.CanvasWidth / 2
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

            if (_settings.RotationAngle != 0)
            {
                (double x, double y) rotatedPosition;

                if (node.Value % 2 == 0)
                {
                    rotatedPosition = RotatePointClockwise(xOffset, yOffset, _settings.RotationAngle);
                }
                else
                {
                    rotatedPosition = RotatePointAntiClockWise(xOffset, yOffset, _settings.RotationAngle);
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
        // Paint for the circle
        var circlePaint = new SKPaint
        {
            Style = SKPaintStyle.Fill,
            Color = GetRandomColor()
        };

        // Paint for the text
        var textPaint = new SKPaint
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            TextSize = 20,
            FakeBoldText = true,
        };

        // Draw the circle
        canvas.DrawCircle(node.Position, 40, circlePaint);

        // Draw the text
        // Adjust the Y coordinate to account for text height (this centers the text vertically in the circle)
        float textY = node.Position.Y + 8;

        canvas.DrawText(node.Value.ToString(), node.Position.X, textY, textPaint);
    }

    private static void SaveCanvas(SKSurface surface, string path)
    {
        Console.WriteLine("Saving image...");

        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 25))
        using (var stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Output saved to: {path}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    private SKColor GetRandomColor()
    {
        byte red = (byte)_random.Next(256);
        byte green = (byte)_random.Next(256);
        byte blue = (byte)_random.Next(256);

        return new SKColor(red, green, blue);
    }

    public (double x, double y) RotatePointAntiClockWise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x - sinTheta * y;
        double yNew = sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }

    public (double x, double y) RotatePointClockwise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x + sinTheta * y;
        double yNew = -sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }
}