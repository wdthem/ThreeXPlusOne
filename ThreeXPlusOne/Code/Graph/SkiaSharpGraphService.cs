using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public class SkiaSharpGraphService(IFileHelper fileHelper,
                                   IConsoleHelper consoleHelper) : IGraphService
{
    private SKSurface? _surface;
    private SKCanvas? _canvas;
    private readonly Random _random = new();

    /// <summary>
    /// Initialize an SKSurface and SKCanvas based on the provided dimensions
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void InitializeGraph(int width, int height)
    {
        DisposeGraphResources();

        _surface = SKSurface.Create(new SKImageInfo(width, height));
        _canvas = _surface.Canvas;

        _canvas.Clear(SKColors.Black);
    }

    /// <summary>
    /// Draw the lines connecting nodes to their parent/children
    /// </summary>
    /// <param name="node"></param>
    public void DrawConnection(DirectedGraphNode node)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not draw connection. Canvas object was null");
        }

        SKPaint paint = new()
        {
            Color = new SKColor(255, 255, 255, 128),
            StrokeWidth = 2,
            IsAntialias = true
        };

        foreach (DirectedGraphNode childNode in node.Children)
        {
            _canvas.DrawLine(new SKPoint(node.Position.X, node.Position.Y),
                             new SKPoint(childNode.Position.X, childNode.Position.Y),
                             paint);
        }
    }

    /// <summary>
    /// Draw the node at its defined position
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    public void DrawNode(DirectedGraphNode node,
                         bool drawNumbersOnNodes,
                         bool distortNodes,
                         int radiusDistortion)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not draw node. Canvas object was null");
        }

        SKPaint paint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = GetRandomNodeColor((byte)_random.Next(30, 211))
        };

        SKPaint textPaint = new()
        {
            Color = SKColors.White,
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            TextAlign = SKTextAlign.Center,
            TextSize = 20,
            FakeBoldText = true,
        };

        if (distortNodes)
        {
            DrawDistortedPath(new SKPoint(node.Position.X, node.Position.Y),
                              node.Radius,
                              radiusDistortion,
                              paint);
        }
        else
        {
            _canvas.DrawCircle(new SKPoint(node.Position.X, node.Position.Y),
                               node.Radius,
                               paint);
        }

        if (drawNumbersOnNodes)
        {
            // Draw the text
            // Adjust the Y coordinate to account for text height (this centers the text vertically in the circle)
            float textY = node.Position.Y + 8;

            _canvas.DrawText(node.Value.ToString(), node.Position.X, textY, textPaint);
        }
    }

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="count"></param>
    public void GenerateBackgroundStars(int count)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not generate background stars. Surface or Canvas object was null");
        }

        List<SKPoint> points = [];

        for (int i = 0; i < count; i++)
        {
            float x = (float)_random.NextDouble() * _canvas.LocalClipBounds.Width;
            float y = (float)_random.NextDouble() * _canvas.LocalClipBounds.Height;

            points.Add(new SKPoint(x, y));
        }

        foreach (SKPoint point in points)
        {
            DrawStarWithBlur(point);
        }
    }

    /// <summary>
    /// Save the generated graph as a png
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void SaveGraphImage()
    {
        if (_surface == null)
        {
            throw new Exception("Could not save SkiaSharp graph. Surface object was null");
        }

        string path = fileHelper.GenerateDirectedGraphFilePath();

        CancellationTokenSource cancellationTokenSource = new();
        CancellationToken token = cancellationTokenSource.Token;

        consoleHelper.WriteLine($"Saving image to: {path}\n");
        consoleHelper.Write("Please wait... ");

        Task spinner = Task.Run(() => consoleHelper.WriteSpinner(token));

        using (SKImage image = _surface.Snapshot())
        using (SKData data = image.Encode(SKEncodedImageFormat.Png, 25))
        using (FileStream stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        cancellationTokenSource.Cancel();

        spinner.Wait();

        DisposeGraphResources();
    }

    /// <summary>
    /// Instead of a circular node, draw a node of a distorted shape based on the user-defined distortion level
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="center"></param>
    /// <param name="baseRadius"></param>
    /// <param name="distortionLevel"></param>
    /// <param name="paint"></param>
    private void DrawDistortedPath(SKPoint center,
                                   float baseRadius,
                                   int distortionLevel,
                                   SKPaint paint)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not draw distorted path. Canvas object was null");
        }

        SKPath path = new();
        int randomPointsCount = _random.Next(3, 11); //from 3 to 10

        path.MoveTo(center.X + baseRadius, center.Y);

        for (int i = 1; i <= randomPointsCount; i++)
        {
            float angle = (float)(2 * Math.PI / randomPointsCount * i);
            float radiusVariation = _random.Next(4, distortionLevel) + 1;
            float radius = baseRadius + radiusVariation;

            SKPoint point = new(center.X + radius * (float)Math.Cos(angle),
                                center.Y + radius * (float)Math.Sin(angle));

            path.LineTo(point);
        }

        path.Close();

        _canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// Apply a blur effect to the stars
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="point"></param>
    private void DrawStarWithBlur(SKPoint point)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not draw star with blur. Canvas object was null");
        }

        float starSize = _random.Next(20, 40);
        float blurRadius = 9.0f;

        SKPaint blurPaint = new()
        {
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, blurRadius)
        };

        SKPaint starPaint = new()
        {
            IsAntialias = true,
            Color = SKColors.White
        };

        _canvas.DrawCircle(point, starSize, blurPaint);
        _canvas.DrawCircle(point, starSize, starPaint);
    }

    private SKColor GetRandomNodeColor(byte alpha = 255)
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
    /// Manually dispose of the graph resources given they are not used in the context of Using statements
    /// </summary>
    private void DisposeGraphResources()
    {
        _canvas?.Dispose();
        _canvas = null;

        _surface?.Dispose();
        _surface = null;
    }
}