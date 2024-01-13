using SkiaSharp;
using System.Collections.ObjectModel;
using System.Drawing;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph.Services;

public class SkiaSharpGraphService(IFileHelper fileHelper,
                                   IConsoleHelper consoleHelper) : IGraphService
{
    private List<DirectedGraphNode>? _nodes;
    private SKSurface? _surface;
    private SKCanvas? _canvas;
    private readonly Random _random = new();

    public GraphProvider GraphProvider => GraphProvider.SkiaSharp;

    public ReadOnlyCollection<int> SupportedDimensions => new(new List<int> { 2, 3 });

    /// <summary>
    /// Initialize an SKSurface and SKCanvas based on the provided dimensions
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void InitializeGraph(List<DirectedGraphNode> nodes,
                                int width,
                                int height)
    {
        DisposeGraphResources();

        _nodes = nodes;
        _surface = SKSurface.Create(new SKImageInfo(width, height));
        _canvas = _surface.Canvas;

        _canvas.Clear(SKColors.Black);

        AddLightSource();
        //AddLensEffect();
    }

    /// <summary>
    /// Draw the graph based on the provided settings
    /// </summary>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="distortNodes"></param>
    /// <param name="drawConnections"></param>
    public void Draw(bool drawNumbersOnNodes,
                     bool distortNodes,
                     bool drawConnections)
    {
        if (_canvas == null || _nodes == null)
        {
            throw new Exception("Could not draw the graph. Canvas object or Nodes object was null");
        }

        var lcv = 0;
        if (drawConnections)
        {
            foreach (var node in _nodes)
            {
                DrawConnection(_canvas, node);

                consoleHelper.Write($"\r{lcv} connections drawn... ");

                lcv += 1;
            }

            consoleHelper.WriteDone();
        }

        lcv = 1;
        foreach (var node in _nodes)
        {
            DrawNode(_canvas,
                     node,
                     drawNumbersOnNodes,
                     distortNodes);

            consoleHelper.Write($"\r{lcv} nodes drawn... ");

            lcv += 1;
        }
    }

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="starCount"></param>
    public void GenerateBackgroundStars(int starCount)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not generate background stars. Surface or Canvas object was null");
        }

        List<SKPoint> points = [];

        for (int i = 0; i < starCount; i++)
        {
            float x = (float)_random.NextDouble() * _canvas.LocalClipBounds.Width;
            float y = (float)_random.NextDouble() * _canvas.LocalClipBounds.Height;

            points.Add(new SKPoint(x, y));
        }

        foreach (SKPoint point in points)
        {
            DrawStarWithBlur(_canvas, point);
        }

        consoleHelper.Write($"{starCount} background stars drawn... ");
        consoleHelper.WriteDone();
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

        consoleHelper.WriteLine($"Saving image to: {path}\n");
        consoleHelper.Write("Please wait... ");

        Task spinner = Task.Run(() => consoleHelper.WriteSpinner(cancellationTokenSource.Token));

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
    /// Draw in individual node
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="distortNodes"></param>
    private void DrawNode(SKCanvas canvas,
                          DirectedGraphNode node,
                          bool drawNumbersOnNodes,
                          bool distortNodes)
    {
        SKPaint paint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = GetNodeSKColor(node.Color)
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
            DrawDistortedPath(canvas,
                              new SKPoint(node.Position.X, node.Position.Y),
                              node.Radius,
                              paint);
        }
        else
        {
            canvas.DrawCircle(new SKPoint(node.Position.X, node.Position.Y),
                              node.Radius,
                              paint);
        }

        if (drawNumbersOnNodes)
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
    /// <param name="paint"></param>
    private void DrawDistortedPath(SKCanvas canvas,
                                   SKPoint center,
                                   float baseRadius,
                                   SKPaint paint)
    {
        SKPath path = new();
        int randomPointsCount = _random.Next(3, 11); //from 3 to 10

        path.MoveTo(center.X + baseRadius, center.Y);

        for (int i = 1; i <= randomPointsCount; i++)
        {
            float angle = (float)(2 * Math.PI / randomPointsCount * i);
            float radiusVariation = _random.Next(5, 26);  //from 5 to 25
            float radius = baseRadius + radiusVariation;

            SKPoint point = new(center.X + radius * (float)Math.Cos(angle),
                                center.Y + radius * (float)Math.Sin(angle));

            path.LineTo(point);
        }

        path.Close();

        canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// Draw the lines connecting nodes to their parent/children
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    private static void DrawConnection(SKCanvas canvas,
                                       DirectedGraphNode node)
    {
        SKPaint paint = new()
        {
            Color = new SKColor(255, 255, 255, 128),
            StrokeWidth = 2,
            IsAntialias = true
        };

        foreach (DirectedGraphNode childNode in node.Children)
        {
            canvas.DrawLine(new SKPoint(node.Position.X, node.Position.Y),
                            new SKPoint(childNode.Position.X, childNode.Position.Y),
                            paint);
        }
    }

    /// <summary>
    /// Apply a blur effect to the stars
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="point"></param>
    private void DrawStarWithBlur(SKCanvas canvas,
                                  SKPoint point)
    {
        float starSize = _random.Next(20, 41); //from 20 to 40
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

        canvas.DrawCircle(point, starSize, blurPaint);
        canvas.DrawCircle(point, starSize, starPaint);
    }

    /// <summary>
    /// Convert the node's color object to an SKColor object
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    private static SKColor GetNodeSKColor(Color color)
    {
        if (color != Color.Empty)
        {
            return new SKColor(color.R,
                               color.G,
                               color.B,
                               color.A);
        }

        //default to white
        return new SKColor(Color.White.R,
                           Color.White.G,
                           Color.White.B,
                           Color.White.A);
    }

    private void AddLightSource()
    {
        if (_canvas == null)
        {
            throw new Exception("Could not add light source. Canvas object was null.");
        }

        SKPoint startPoint = new(0, 0); // Top left corner
        SKPoint endPoint = new(_canvas.LocalClipBounds.Width, _canvas.LocalClipBounds.Height);

        SKColor startColor = SKColors.LightYellow; // Bright color for the light source
        SKColor endColor = SKColors.Black;

        SKShader shader = SKShader.CreateLinearGradient(startPoint,
                                                        endPoint,
                                                        [startColor, endColor],
                                                        [0, 0.75f], // Corresponding to start and end colors
                                                        SKShaderTileMode.Clamp);

        SKPaint paint = new()
        {
            Shader = shader
        };

        _canvas.DrawRect(0,
                         0,
                         _canvas.LocalClipBounds.Width,
                         _canvas.LocalClipBounds.Height,
                         paint);
    }

    private void AddLensEffect()
    {
        if (_canvas == null)
        {
            throw new Exception("Could not add lens effect. Canvas object was null.");
        }

        // Primary light source - a bright radial gradient
        SKPoint lightSourceCenter = new(_canvas.LocalClipBounds.Width / 2, _canvas.LocalClipBounds.Height / 2); // Position of the light source

        float lightRadius = 750; // Radius of the bright spot

        SKShader lightShader = SKShader.CreateRadialGradient(lightSourceCenter,
                                                             lightRadius,
                                                             new[] { SKColors.LightYellow, SKColors.Transparent },
                                                             null,
                                                             SKShaderTileMode.Clamp);

        _canvas.DrawCircle(lightSourceCenter.X, lightSourceCenter.Y, lightRadius, new SKPaint { Shader = lightShader });

        // Simple halo - an ellipse with a gradient
        SKPoint haloCenter = new(_canvas.LocalClipBounds.Width / 2, _canvas.LocalClipBounds.Height / 2); // Same as light source, or slightly offset

        float haloWidth = 1500;
        float haloHeight = 750;

        SKRect haloRect = new(haloCenter.X - haloWidth, haloCenter.Y - haloHeight, haloCenter.X + haloWidth, haloCenter.Y + haloHeight);

        SKShader haloShader = SKShader.CreateRadialGradient(haloCenter,
                                                            haloWidth,
                                                            new[] { SKColors.Transparent, SKColors.Yellow.WithAlpha(128), SKColors.Transparent },
                                                            null,
                                                            SKShaderTileMode.Clamp);

        _canvas.DrawOval(haloRect, new SKPaint { Shader = haloShader });
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

        _nodes = null;
    }
}