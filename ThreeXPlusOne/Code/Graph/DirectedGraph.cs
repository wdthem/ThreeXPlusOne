﻿using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public abstract class DirectedGraph(IOptions<Settings> settings,
                                    IFileHelper fileHelper,
                                    IConsoleHelper consoleHelper)
{
    protected readonly IOptions<Settings> _settings = settings;
    protected readonly IFileHelper _fileHelper = fileHelper;
    protected readonly IConsoleHelper _consoleHelper = consoleHelper;
    protected readonly Random _random = new();
    protected readonly Dictionary<int, DirectedGraphNode> _nodes = [];

    /// <summary>
    /// Add a series of numbers to the graph generated by the algorithm
    /// </summary>
    /// <param name="series"></param>
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
                    Depth = currentDepth
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

            currentDepth--;
        }

        var maxNodeDepth = _nodes.Max(node => node.Value.Depth);


        foreach (var node in _nodes)
        {
            float z = maxNodeDepth - node.Value.Depth;

            node.Value.Z = z;
        }
    }

    /// <summary>
    /// Draw the graph
    /// </summary>
    protected void Draw()
    {
        _consoleHelper.WriteLine("Drawing connections and nodes... ");

        using var surface = SKSurface.Create(new SKImageInfo(_settings.Value.CanvasWidth, _settings.Value.CanvasHeight));

        SKCanvas canvas = surface.Canvas;

        canvas.Clear(SKColors.Black);

        if (_settings.Value.GenerateBackgroundStars)
        {
            GenerateBackgroundStars(canvas, 100);
        }

        var lcv = 1;

        if (_settings.Value.DrawConnections)
        {
            foreach (var node in _nodes)
            {
                DrawConnection(canvas, node.Value);

                _consoleHelper.Write($"    \r{lcv} connections drawn");

                lcv += node.Value.Children.Count;
            }

            _consoleHelper.WriteLine("");
        }

        lcv = 1;
        foreach (var node in _nodes)
        {
            DrawNode(canvas, node.Value);

            _consoleHelper.Write($"    \r{lcv} nodes drawn");

            lcv = lcv + 1 + node.Value.Children.Count;
        }

        _consoleHelper.WriteLine("");
        _consoleHelper.WriteDone();

        if (_settings.Value.GenerateGraph)
        {
            bool confirmed = _consoleHelper.ReadYKeyToProceed($"Generate {_settings.Value.GraphDimensions}D visualization?");

            if (!confirmed)
            {
                _consoleHelper.WriteLine("\nGraph generation cancelled");

                return;
            }

            _consoleHelper.WriteLine("");

            SaveCanvas(surface);
        }
        else
        {
            _consoleHelper.WriteLine("Graph generation disabled");
        }
    }

    /// <summary>
    /// Draw the lines connecting nodes to their parent/children
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
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

    /// <summary>
    /// Draw the node at its defined position
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    private void DrawNode(SKCanvas canvas, DirectedGraphNode node)
    {
        var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = GetRandomNodeColor(128)
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

    /// <summary>
    /// Instead of a circular node, draw a node of a distorted shape based on the user-defined distortion level
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="center"></param>
    /// <param name="baseRadius"></param>
    /// <param name="distortionLevel"></param>
    /// <param name="paint"></param>
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

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="count"></param>
    private void GenerateBackgroundStars(SKCanvas canvas, int count)
    {
        var points = new List<SKPoint>();

        for (int i = 0; i < count; i++)
        {
            float x = (float)_random.NextDouble() * _settings.Value.CanvasWidth;
            float y = (float)_random.NextDouble() * _settings.Value.CanvasHeight;

            points.Add(new SKPoint(x, y));
        }

        var lcv = 1;
        foreach (var point in points)
        {
            if (lcv % 7 == 0)
            {
                DrawStarWithTrails(canvas, point);
            }
            else
            {
                DrawStarWithBlur(canvas, point);
            }

            lcv++;
        }
    }

    /// <summary>
    /// Apply a blur effect to the stars
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="point"></param>
    private void DrawStarWithBlur(SKCanvas canvas, SKPoint point)
    {
        float starSize = _random.Next(20, 40);
        float blurRadius = 9.0f;

        var blurPaint = new SKPaint
        {
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius)
        };

        var starPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.White
        };

        canvas.DrawCircle(point, starSize, blurPaint);
        canvas.DrawCircle(point, starSize, starPaint);
    }

    /// <summary>
    /// Draw stars with trails coming from the center
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="center"></param>
    private void DrawStarWithTrails(SKCanvas canvas, SKPoint center)
    {
        float starSize = _random.Next(20, 40);
        float trailLength = starSize + 50; // Length of the light trails
        float trailStartWidth = starSize / 2; // Starting width of the light trails
        float trailEndWidth = starSize / 10; // Ending width (tip) of the light trails
        float blurRadius = 9.0f;

        var starPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.White
        };

        var trailPaint = new SKPaint
        {
            IsAntialias = true,
            Color = SKColors.White,
            Style = SKPaintStyle.StrokeAndFill
        };

        var blurPaint = new SKPaint
        {
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius)
        };

        canvas.DrawCircle(center, starSize / 2, blurPaint);
        canvas.DrawCircle(center, starSize / 2, starPaint);

        // Draw trails in all four directions
        DrawTaperedTrail(canvas, center, new SKPoint(center.X, center.Y - trailLength), trailStartWidth, trailEndWidth, trailPaint); // Up
        DrawTaperedTrail(canvas, center, new SKPoint(center.X, center.Y + trailLength), trailStartWidth, trailEndWidth, trailPaint); // Down
        DrawTaperedTrail(canvas, center, new SKPoint(center.X - trailLength, center.Y), trailStartWidth, trailEndWidth, trailPaint); // Left
        DrawTaperedTrail(canvas, center, new SKPoint(center.X + trailLength, center.Y), trailStartWidth, trailEndWidth, trailPaint); // Right
    }

    /// <summary>
    /// Draw the trails such that they taper off toward the outside
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="startWidth"></param>
    /// <param name="endWidth"></param>
    /// <param name="paint"></param>
    private static void DrawTaperedTrail(SKCanvas canvas, SKPoint start, SKPoint end, float startWidth, float endWidth, SKPaint paint)
    {
        using var path = new SKPath();

        // Calculate the direction of the trail
        var direction = new SKPoint(end.X - start.X, end.Y - start.Y);
        var perpendicular = NormalizeVector(new SKPoint(-direction.Y, direction.X));

        // Calculate the four corners of the trail
        var corner1 = new SKPoint(start.X + perpendicular.X * (startWidth / 2), start.Y + perpendicular.Y * (startWidth / 2));
        var corner2 = new SKPoint(start.X - perpendicular.X * (startWidth / 2), start.Y - perpendicular.Y * (startWidth / 2));
        var corner3 = new SKPoint(end.X - perpendicular.X * (endWidth / 2), end.Y - perpendicular.Y * (endWidth / 2));
        var corner4 = new SKPoint(end.X + perpendicular.X * (endWidth / 2), end.Y + perpendicular.Y * (endWidth / 2));

        // Draw the tapered trail
        path.MoveTo(corner1);
        path.LineTo(corner2);
        path.LineTo(corner3);
        path.LineTo(corner4);
        path.Close();

        canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// Apply a minor position adjustment to nodes that share the same position
    /// </summary>
    /// <param name="nodes"></param>
    protected void AdjustNodesWithSamePosition(List<DirectedGraphNode> nodes)
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

    /// <summary>
    /// Get a random non-black color for the given node
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    protected SKColor GetRandomNodeColor(byte alpha = 255)
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

    /// <summary>
    /// Rotate the node's position anti-clockwise based on the angle provided by the user. This gives a more artistic feel to the generated graph.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    protected static (double x, double y) RotatePointAntiClockWise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x - sinTheta * y;
        double yNew = sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }

    /// <summary>
    /// Rotate the node's position clockwise based on the angle provided by the user. This gives a more artistic feel to the generated graph.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    protected static (double x, double y) RotatePointClockwise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x + sinTheta * y;
        double yNew = -sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }

    /// <summary>
    /// Normalise a vector to aid in drawing
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    private static SKPoint NormalizeVector(SKPoint vector)
    {
        float length = (float)Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);

        if (length > 0)
        {
            return new SKPoint(vector.X / length, vector.Y / length);
        }

        return vector;
    }

    /// <summary>
    /// Flatten the node and its children to help look across all node properties
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Save the generated canvas
    /// </summary>
    /// <param name="surface"></param>
    protected void SaveCanvas(SKSurface surface)
    {
        _consoleHelper.WriteLine("");
        _consoleHelper.Write("Saving image... ");

        string path = _fileHelper.GenerateDirectedGraphFilePath();

        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 25))
        using (var stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        _consoleHelper.WriteLine("");
        Console.ForegroundColor = ConsoleColor.Green;
        _consoleHelper.WriteLine($"Saved to: {path}");
        Console.ForegroundColor = ConsoleColor.White;
    }
}