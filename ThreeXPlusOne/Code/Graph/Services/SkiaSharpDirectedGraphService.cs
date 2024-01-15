using SkiaSharp;
using System.Collections.ObjectModel;
using System.Drawing;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph.Services;

public class SkiaSharpDirectedGraphService(IFileHelper fileHelper,
                                           IConsoleHelper consoleHelper) : IDirectedGraphService
{
    private List<DirectedGraphNode>? _nodes;
    private SKSurface? _surface;
    private SKCanvas? _canvas;
    private SKImage? _image;
    private int _dimensions;
    private bool _lightSourceInPlace = false;
    private (float X, float Y) _lightSourceOrigin;
    private readonly Random _random = new();

    public GraphProvider GraphProvider => GraphProvider.SkiaSharp;

    public ReadOnlyCollection<int> SupportedDimensions => new([2, 3]);

    /// <summary>
    /// Initialize an SKSurface and SKCanvas based on the provided dimensions
    /// </summary>
    /// <param name="nodes"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="dimensions"></param>
    public void Initialize(List<DirectedGraphNode> nodes,
                           int width,
                           int height,
                           int dimensions)
    {
        CancellationTokenSource cancellationTokenSource = new();

        consoleHelper.Write($"Initializing {GraphProvider} graph... ");

        Task spinner = Task.Run(() => consoleHelper.WriteSpinner(cancellationTokenSource.Token));

        _nodes = nodes;
        _surface = SKSurface.Create(new SKImageInfo(width, height));
        _canvas = _surface.Canvas;
        _dimensions = dimensions;

        _canvas.Clear(SKColors.Black);

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
    /// <exception cref="Exception"></exception>
    public void GenerateLightSource()
    {
        if (_canvas == null)
        {
            throw new Exception("Could not add light source. Canvas object was null.");
        }

        CancellationTokenSource cancellationTokenSource = new();

        consoleHelper.Write("Generating light source... ");

        Task spinner = Task.Run(() => consoleHelper.WriteSpinner(cancellationTokenSource.Token));

        _lightSourceOrigin = (0, 0);
        _lightSourceInPlace = true;

        SKPoint startPoint = new(_lightSourceOrigin.X, _lightSourceOrigin.Y);
        SKPoint endPoint = new(_canvas.LocalClipBounds.Width, _canvas.LocalClipBounds.Height);

        SKColor startColor = SKColors.LightYellow.WithAlpha(200);
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

        cancellationTokenSource.Cancel();

        spinner.Wait();

        consoleHelper.WriteDone();
    }

    /// <summary>
    /// Draw the graph based on the provided settings
    /// </summary>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="usePolygonsAsNodes"></param>
    /// <param name="drawConnections"></param>
    /// <exception cref="Exception"></exception>
    public void Draw(bool drawNumbersOnNodes,
                     bool usePolygonsAsNodes,
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
                     usePolygonsAsNodes);

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
    /// <param name="usePolygonsAsNodes"></param>
    private void DrawNode(SKCanvas canvas,
                          DirectedGraphNode node,
                          bool drawNumbersOnNodes,
                          bool usePolygonsAsNodes)
    {
        SKPaint paint = GenerateNodePaint(node);

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
                  paint,
                  usePolygonsAsNodes);

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
    /// <param name="usePolygonsAsNodes"></param>
    private void DrawShape(SKCanvas canvas,
                           DirectedGraphNode node,
                           SKPaint paint,
                           bool usePolygonsAsNodes)
    {
        int polygonSides = _random.Next(0, 11);

        if (!usePolygonsAsNodes || polygonSides == 0)
        {
            node.Shape.ShapeType = ShapeType.Circle;

            canvas.DrawCircle(new SKPoint(node.Position.X, node.Position.Y),
                              node.Shape.Radius,
                              paint);

            return;
        }

        float rotationAngle = (float)(_random.NextDouble() * 2 * Math.PI);

        if (polygonSides == 1 || polygonSides == 2)
        {
            polygonSides = _random.Next(3, 11); //cannot have 1 or 2 sides, so re-select
        }

        SKPath path = new();
        node.Shape.ShapeType = ShapeType.Polygon;

        for (int i = 0; i < polygonSides; i++)
        {
            float angle = (float)(2 * Math.PI / polygonSides * i) + rotationAngle;

            SKPoint point = new(node.Position.X + node.Shape.Radius * (float)Math.Cos(angle),
                                node.Position.Y + node.Shape.Radius * (float)Math.Sin(angle));

            if (i == 0)
            {
                path.MoveTo(point);
            }
            else
            {
                path.LineTo(point);
            }

            node.Shape.Vertices.Add((point.X, point.Y));
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
    /// Get the node SKPaint applying a light source as appropriate
    /// </summary>
    /// <param name="alpha"></param>
    /// <returns></returns>
    private SKPaint GenerateNodePaint(DirectedGraphNode node)
    {
        //default to white
        SKColor nodeBaseColor = new(Color.White.R,
                                    Color.White.G,
                                    Color.White.B,
                                    Color.White.A);

        if (node.Shape.Color != Color.Empty)
        {
            nodeBaseColor = new SKColor(node.Shape.Color.R,
                                        node.Shape.Color.G,
                                        node.Shape.Color.B,
                                        node.Shape.Color.A);
        }

        SKColor nodeColor = nodeBaseColor;

        if (_lightSourceInPlace)
        {
            nodeColor = ApplyLightSourceToNodeColor(node, nodeBaseColor);
        }

        return new SKPaint()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = nodeColor
        };
    }

    /// <summary>
    /// If a light source is in place, it should impact the colour of nodes.
    /// The closer to the source, the more the impact.
    /// </summary>
    /// <param name="node"></param>
    /// <param name="nodeBaseColor"></param>
    /// <returns></returns>
    private SKColor ApplyLightSourceToNodeColor(DirectedGraphNode node,
                                                SKColor nodeBaseColor)
    {
        SKColor nodeColor;
        float distance = Distance(new SKPoint(node.Position.X, node.Position.Y),
                                  new SKPoint(_lightSourceOrigin.X, _lightSourceOrigin.Y));


        float additionalOpacityFactor;
        float maxDistance = _canvas!.LocalClipBounds.Height / (float)1.2;

        float lightIntensity = 0.5f; // Adjust this value between 0 and 1 to control the light's power


        if (distance < maxDistance)
        {
            additionalOpacityFactor = distance / maxDistance;
            additionalOpacityFactor = Math.Clamp(additionalOpacityFactor, 0, 1);

            // Apply the light intensity to the blend factor
            float blendFactor = additionalOpacityFactor * lightIntensity;
            nodeColor = BlendColor(nodeBaseColor, SKColors.LightYellow, 1 - blendFactor);
        }
        else
        {
            nodeColor = nodeBaseColor;
            additionalOpacityFactor = 1.0f;
        }

        byte finalAlpha = (byte)(nodeBaseColor.Alpha * additionalOpacityFactor);

        return nodeColor.WithAlpha(finalAlpha);
    }

    /// <summary>
    /// Calculate the Euclidean distance between two node positions
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns></returns>
    private static float Distance(SKPoint position1, SKPoint position2)
    {
        return (float)Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
    }

    /// <summary>
    /// Blend the node's colour with the light source, adjusted for distance from the light source
    /// </summary>
    /// <param name="baseColor"></param>
    /// <param name="blendColor"></param>
    /// <param name="blendFactor"></param>
    /// <returns></returns>
    private static SKColor BlendColor(SKColor baseColor,
                                      SKColor blendColor,
                                      float blendFactor)
    {
        byte r = (byte)((baseColor.Red * (1 - blendFactor)) + (blendColor.Red * blendFactor));
        byte g = (byte)((baseColor.Green * (1 - blendFactor)) + (blendColor.Green * blendFactor));
        byte b = (byte)((baseColor.Blue * (1 - blendFactor)) + (blendColor.Blue * blendFactor));

        return new SKColor(r, g, b);
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