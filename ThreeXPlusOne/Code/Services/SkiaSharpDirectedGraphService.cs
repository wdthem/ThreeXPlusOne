using SkiaSharp;
using System.Collections.ObjectModel;
using System.Drawing;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Enums;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Services;

public class SkiaSharpDirectedGraphService(IFileHelper fileHelper,
                                           IConsoleHelper consoleHelper) : IDirectedGraphService
{
    private List<DirectedGraphNode>? _nodes;
    private SKSurface? _surface;
    private SKCanvas? _canvas;
    private SKImage? _image;
    private SKColor _canvasBackgroundColor;
    private readonly Random _random = new();

    public GraphProvider GraphProvider => GraphProvider.SkiaSharp;

    public ReadOnlyCollection<int> SupportedDimensions => new([2, 3]);

    /// <summary>
    /// Initialize an SKSurface and SKCanvas based on the provided dimensions
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="backgroundColor"></param>
    public void Initialize(List<DirectedGraphNode> nodes,
                           int width,
                           int height,
                           Color backgroundColor)
    {
        CancellationTokenSource cancellationTokenSource = new();

        consoleHelper.Write($"Initializing {GraphProvider} graph... ");

        Task spinner = Task.Run(() => consoleHelper.WriteSpinner(cancellationTokenSource.Token));

        _nodes = nodes;
        _surface = SKSurface.Create(new SKImageInfo(width, height));
        _canvas = _surface.Canvas;
        _canvasBackgroundColor = GenerateSKColor(backgroundColor);

        _canvas.Clear(_canvasBackgroundColor);

        cancellationTokenSource.Cancel();

        spinner.Wait();

        consoleHelper.WriteDone();
    }

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="starCount"></param>
    /// <exception cref="Exception"></exception>
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
    /// Add a light source from the top left
    /// </summary>
    /// <param name="lightSourceCoordinates"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <exception cref="Exception"></exception>
    public void GenerateLightSource((float X, float Y) lightSourceCoordinates,
                                    float radius,
                                    Color color)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not add light source. Canvas object was null.");
        }

        CancellationTokenSource cancellationTokenSource = new();

        consoleHelper.Write("Generating light source... ");

        Task spinner = Task.Run(() => consoleHelper.WriteSpinner(cancellationTokenSource.Token));

        SKColor startColor = GenerateSKColor(color);
        SKColor endColor = _canvasBackgroundColor;

        // Create a radial gradient from the specified origin
        SKShader shader = SKShader.CreateRadialGradient(new SKPoint(lightSourceCoordinates.X, lightSourceCoordinates.Y),
                                                        radius,
                                                        [startColor, endColor],
                                                        [0, 0.75f], // Gradient stops
                                                        SKShaderTileMode.Clamp);

        SKPaint paint = new()
        {
            Shader = shader
        };

        // Draw the rectangle over the entire canvas
        _canvas.DrawRect(_canvas.LocalClipBounds, paint);

        cancellationTokenSource.Cancel();

        spinner.Wait();

        consoleHelper.WriteDone();
    }

    /// <summary>
    /// Draw the graph based on the provided settings
    /// </summary>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="drawNodeConnections"></param>
    /// <exception cref="Exception"></exception>
    public void Draw(bool drawNumbersOnNodes,
                     bool drawNodeConnections)
    {
        if (_canvas == null || _nodes == null)
        {
            throw new Exception("Could not draw the graph. Canvas object or Nodes object was null");
        }

        var lcv = 0;
        if (drawNodeConnections)
        {
            foreach (var node in _nodes)
            {
                DrawNodeConnection(_canvas, node);

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
                     drawNumbersOnNodes);

            consoleHelper.Write($"\r{lcv} nodes drawn... ");

            lcv += 1;
        }

        consoleHelper.WriteDone();
    }

    /// <summary>
    /// Render the graph
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void Render()
    {
        if (_surface == null)
        {
            throw new Exception("Could not render graph. Surface object was null");
        }

        consoleHelper.Write($"Rendering graph... ");

        _image = _surface.Snapshot();

        consoleHelper.WriteDone();
    }

    /// <summary>
    /// Save the generated graph as a PNG
    /// </summary>
    /// <exception cref="Exception"></exception>
    public void SaveImage()
    {
        if (_image == null)
        {
            throw new Exception("Could not save graph. Image object was null");
        }

        string path = fileHelper.GenerateDirectedGraphFilePath();

        CancellationTokenSource cancellationTokenSource = new();

        consoleHelper.Write($"Saving image... ");

        Task spinner = Task.Run(() => consoleHelper.WriteSpinner(cancellationTokenSource.Token));

        using (SKData data = _image.Encode(SKEncodedImageFormat.Png, 25))
        using (FileStream stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        cancellationTokenSource.Cancel();

        spinner.Wait();

        consoleHelper.WriteDone();

        consoleHelper.WriteLine($"Image saved to {path}\n");
    }

    /// <summary>
    /// Draw in individual node
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    /// <param name="drawNumbersOnNodes"></param>
    private static void DrawNode(SKCanvas canvas,
                                 DirectedGraphNode node,
                                 bool drawNumbersOnNodes)
    {
        SKPaint paint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = GenerateSKColor(node.Shape.Color)
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

        DrawShape(canvas,
                  node,
                  paint);

        if (drawNumbersOnNodes)
        {
            // Draw the text
            // Adjust the Y coordinate to account for text height (this centers the text vertically in the circle)
            float textY = node.Position.Y + 8;

            canvas.DrawText(node.Value.ToString(), node.Position.X, textY, textPaint);
        }
    }

    /// <summary>
    /// Draw either a circle or a polygon for the node based on settings
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    /// <param name="paint"></param>
    private static void DrawShape(SKCanvas canvas,
                                  DirectedGraphNode node,
                                  SKPaint paint)
    {
        if (node.Shape.ShapeType == ShapeType.Circle)
        {
            canvas.DrawCircle(new SKPoint(node.Position.X, node.Position.Y),
                              node.Shape.Radius,
                              paint);

            return;
        }

        SKPath path = new();

        for (int i = 0; i < node.Shape.Vertices.Count; i++)
        {
            (float x, float y) = node.Shape.Vertices[i];

            if (i == 0)
            {
                path.MoveTo(new SKPoint(x, y));
            }
            else
            {
                path.LineTo(new SKPoint(x, y));
            }
        }

        path.Close();
        canvas.DrawPath(path, paint);
    }

    /// <summary>
    /// Draw the lines connecting nodes to their parent/children
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    private static void DrawNodeConnection(SKCanvas canvas,
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
    /// Get the node SKColor from a Color object
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static SKColor GenerateSKColor(Color color)
    {
        //default to white
        SKColor nodeColor = new(Color.White.R,
                                Color.White.G,
                                Color.White.B,
                                Color.White.A);

        if (color != Color.Empty)
        {
            nodeColor = new SKColor(color.R,
                                    color.G,
                                    color.B,
                                    color.A);
        }

        return nodeColor;
    }

    #region IDisposable

    /// <summary>
    /// IDisposable Dispose method
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Method to handle disposing resources
    /// </summary>
    /// <param name="disposing"></param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _image?.Dispose();
            _image = null;

            _canvas?.Dispose();
            _canvas = null;

            _surface?.Dispose();
            _surface = null;

            _nodes = null;
        }
    }

    /// <summary>
    /// IDisposable finalizer
    /// </summary>
    ~SkiaSharpDirectedGraphService()
    {
        Dispose(false);
    }

    #endregion
}