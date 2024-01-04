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
    protected readonly Settings _settings = settings.Value;
    protected readonly IFileHelper _fileHelper = fileHelper;
    protected readonly IConsoleHelper _consoleHelper = consoleHelper;
    protected readonly Random _random = new();
    protected readonly Dictionary<int, DirectedGraphNode> _nodes = [];
    private readonly Dictionary<(int, int), List<SKPoint>> _nodeGrid = [];
    private int _canvasWidth = 0;
    private int _canvasHeight = 0;

    /// <summary>
    /// Add multiple series of numbers to the graph generated by the algorithm
    /// </summary>
    /// <param name="seriesLists"></param>
    public void AddSeries(List<List<int>> seriesLists)
    {
        _consoleHelper.Write($"Adding {seriesLists.Count} series to the graph... ");

        foreach (List<int> series in seriesLists)
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
                node.Value.Z = maxNodeDepth - node.Value.Depth;
            }
        }

        _consoleHelper.WriteDone();
    }

    /// <summary>
    /// Draw the graph
    /// </summary>
    protected void DrawGraph()
    {
        MoveNodesToPositiveCoordinates();
        SetCanvasSize();

        using var surface = SKSurface.Create(new SKImageInfo(_canvasWidth, _canvasHeight));

        SKCanvas canvas = surface.Canvas;

        canvas.Clear(SKColors.Black);

        if (_settings.GenerateBackgroundStars)
        {
            GenerateBackgroundStars(canvas, 100);
        }

        var lcv = 0;
        if (_settings.DrawConnections)
        {
            foreach (var node in _nodes)
            {
                DrawConnection(canvas, node.Value);

                _consoleHelper.Write($"\r{lcv} connections drawn... ");

                lcv += 1;
            }

            _consoleHelper.WriteDone();
        }

        lcv = 1;
        foreach (var node in _nodes)
        {
            DrawNode(canvas, node.Value);

            _consoleHelper.Write($"\r{lcv} nodes drawn... ");

            lcv += 1;
        }

        _consoleHelper.WriteDone();

        bool confirmedGenerateGraph = _consoleHelper.ReadYKeyToProceed($"Generate {_settings.GraphDimensions}D visualization?");

        if (!confirmedGenerateGraph)
        {
            _consoleHelper.WriteLine("\nGraph generation cancelled\n");

            return;
        }

        _consoleHelper.WriteLine("");

        SaveCanvas(surface);
    }

    /// <summary>
    /// Determine if the node that was just positioned is too close to neighbouring nodes (and thus overlapping)
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    protected bool NodeIsTooCloseToNeighbours(DirectedGraphNode newNode, float minDistance)
    {
        var cell = GetGridCellForNode(newNode, minDistance);

        // Check this cell and perhaps adjacent cells
        foreach (var offset in new[] { (0, 0), (1, 0), (0, 1), (-1, 0), (0, -1) })
        {
            var checkCell = (cell.Item1 + offset.Item1, cell.Item2 + offset.Item2);

            if (_nodeGrid.TryGetValue(checkCell, out var nodesInCell))
            {
                foreach (var node in nodesInCell)
                {
                    if (Distance(newNode.Position, node) < minDistance)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Add the node to the grid dictionary to keep track of positions in a grid system
    /// </summary>
    /// <param name="node"></param>
    /// <param name="minDistance"></param>
    protected void AddNodeToGrid(DirectedGraphNode node, float minDistance)
    {
        var cell = GetGridCellForNode(node, minDistance);

        if (!_nodeGrid.TryGetValue(cell, out List<SKPoint>? value))
        {
            value = ([]);
            _nodeGrid[cell] = value;
        }

        value.Add(node.Position);
    }

    /// <summary>
    /// Calculate the Euclidean distance between two nodes
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    protected static float Distance(SKPoint point1, SKPoint point2)
    {
        return (float)Math.Sqrt(Math.Pow(point2.X - point1.X, 2) + Math.Pow(point2.Y - point1.Y, 2));
    }

    protected static float XAxisSignedDistanceFromParent(SKPoint child, SKPoint parent)
    {
        return child.X - parent.X;
    }

    /// <summary>
    /// Retrieve the cell in the grid object in which the node is positioned
    /// </summary>
    /// <param name="node"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    private static (int, int) GetGridCellForNode(DirectedGraphNode node, float cellSize)
    {
        return ((int)(node.Position.X / cellSize), (int)(node.Position.Y / cellSize));
    }

    /// <summary>
    /// The graph starts out at 0,0 with 0 width and 0 height. This means that nodes go into negative space, so all 
    /// coordinates need to be shifted to make sure all are in positive space
    /// </summary>
    private void MoveNodesToPositiveCoordinates()
    {
        _consoleHelper.Write("Adjusting node positions to fit on canvas... ");

        var minX = _nodes.Min(node => node.Value.Position.X);
        var minY = _nodes.Min(node => node.Value.Position.Y);

        var translationX = minX < 0 ? -minX + 500 : 0;
        var translationY = minY < 0 ? -minY + 500 : 0;

        foreach (var node in _nodes)
        {
            node.Value.Position = new SKPoint(node.Value.Position.X + translationX,
                                              node.Value.Position.Y + translationY);
        }

        _consoleHelper.WriteDone();
    }

    /// <summary>
    /// Set the canvas width to a bit more than the bounding box of all the nodes
    /// </summary>
    private void SetCanvasSize()
    {
        var maxX = _nodes.Max(node => node.Value.Position.X);
        var maxY = _nodes.Max(node => node.Value.Position.Y);

        _canvasWidth = (int)maxX + 500;
        _canvasHeight = (int)maxY + 500;

        _consoleHelper.WriteLine($"Canvas dimensions set to {_canvasWidth}w x {_canvasHeight}h (in pixels)\n");
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
            Color = GetRandomNodeColor((byte)_random.Next(30, 211))
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

        if (_settings.DistortNodes)
        {
            DrawDistortedPath(canvas,
                              node.Position,
                              node.Radius,
                              _settings.RadiusDistortion,
                              paint);
        }
        else
        {
            canvas.DrawCircle(node.Position,
                              node.Radius,
                              paint);
        }

        if (_settings.DrawNumbersOnNodes)
        {
            // Draw the text
            // Adjust the Y coordinate to account for text height (this centers the text vertically in the circle)
            float textY = node.Position.Y + 8;

            canvas.DrawText(node.Value.ToString(), node.Position.X, textY, textPaint);
        }
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
        var randomPointsCount = _random.Next(3, 11); //from 3 to 10

        path.MoveTo(center.X + baseRadius, center.Y);

        for (int i = 1; i <= randomPointsCount; i++)
        {
            float angle = (float)(2 * Math.PI / randomPointsCount * i);
            float radiusVariation = _random.Next(4, distortionLevel) + 1;
            float radius = baseRadius + radiusVariation;

            var point = new SKPoint(center.X + radius * (float)Math.Cos(angle),
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
            float x = (float)_random.NextDouble() * _canvasWidth;
            float y = (float)_random.NextDouble() * _canvasHeight;

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
        while (red <= 10 || green <= 10 || blue <= 10); //avoid very dark colours

        return new SKColor(red, green, blue, alpha);
    }

    /// <summary>
    /// Rotate the node's position anti-clockwise based on the angle provided by the user. This gives a more artistic feel to the generated graph.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    protected static (double x, double y) RotatePointAntiClockwise(double x, double y, double angleDegrees)
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
    /// Save the generated canvas
    /// </summary>
    /// <param name="surface"></param>
    protected void SaveCanvas(SKSurface surface)
    {
        string path = _fileHelper.GenerateDirectedGraphFilePath();

        var cancellationTokenSource = new CancellationTokenSource();
        var token = cancellationTokenSource.Token;

        _consoleHelper.WriteLine($"Saving image to: {path}\n");
        _consoleHelper.Write("Please wait... ");

        Task spinner = Task.Run(() => _consoleHelper.WriteSpinner(token));

        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 25))
        using (var stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        cancellationTokenSource.Cancel();

        spinner.Wait();
    }
}