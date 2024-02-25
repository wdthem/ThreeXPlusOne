using SkiaSharp;
using System.Collections.ObjectModel;
using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.Services.SkiaSharp;

public partial class SkiaSharpDirectedGraphService(IFileService fileService) : IDirectedGraphService
{
    private List<DirectedGraphNode>? _nodes;
    private SKSurface? _surface;
    private SKCanvas? _canvas;
    private SKImage? _image;

    public Action<string>? OnStart { get; set; }
    public Action? OnComplete { get; set; }

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
        OnStart?.Invoke($"Initializing {GraphProvider} graph... ");

        _nodes = nodes;
        _surface = SKSurface.Create(new SKImageInfo(width, height));
        _canvas = _surface.Canvas;

        _canvas.Clear(ConvertColorToSKColor(backgroundColor));

        OnComplete?.Invoke();
    }

    /// <summary>
    /// Optionally generate white points in the background to mimic stars
    /// </summary>
    /// <param name="starCount"></param>
    /// <param name="nodeRadius"></param>
    /// <exception cref="Exception"></exception>
    public void GenerateBackgroundStars(int starCount, double nodeRadius)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not generate background stars. Surface or Canvas object was null");
        }

        OnStart?.Invoke($"Drawing {starCount} background stars... ");

        List<SKPoint> points = [];

        for (int i = 0; i < starCount; i++)
        {
            double x = Random.Shared.NextDouble() * _canvas.LocalClipBounds.Width;
            double y = Random.Shared.NextDouble() * _canvas.LocalClipBounds.Height;

            points.Add(ConvertCoordinatesToSKPoint((x, y)));
        }

        foreach (SKPoint point in points)
        {
            DrawStarWithBlur(_canvas,
                             point);
        }

        OnComplete?.Invoke();
    }

    /// <summary>
    /// Add a light source from the top left
    /// </summary>
    /// <param name="lightSourceCoordinates"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <exception cref="Exception"></exception>
    public void GenerateLightSource((double X, double Y) lightSourceCoordinates,
                                    double radius,
                                    Color color)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not add light source. Canvas object was null.");
        }

        OnStart?.Invoke("Generating light source... ");

        SKColor startColor = ConvertColorToSKColor(color);
        SKColor endColor = SKColors.Transparent;

        // Create a radial gradient from the specified origin
        SKShader shader = SKShader.CreateRadialGradient(ConvertCoordinatesToSKPoint(lightSourceCoordinates),
                                                        (float)radius,
                                                        [startColor, endColor],
                                                        [0, 0.75f], // Gradient stops
                                                        SKShaderTileMode.Clamp);

        using SKPaint paint = new()
        {
            Shader = shader
        };

        // Draw the rectangle over the entire canvas
        _canvas.DrawRect(_canvas.LocalClipBounds, paint);

        OnComplete?.Invoke();
    }

    /// <summary>
    /// Draw the graph based on the provided app settings
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

        string extraMessage = "";

        if (drawNodeConnections)
        {
            extraMessage = " and connections";
        }

        OnStart?.Invoke($"Drawing {_nodes.Count} nodes{extraMessage}... ");

        if (drawNodeConnections)
        {
            //do a separate loop to draw connections to ensure they are drawn under nodes
            foreach (DirectedGraphNode node in _nodes)
            {
                DrawNodeConnection(_canvas, node);
            }
        }

        foreach (DirectedGraphNode node in _nodes)
        {
            DrawNode(_canvas,
                     node,
                     drawNumbersOnNodes);
        }

        OnComplete?.Invoke();
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

        OnStart?.Invoke("Rendering graph... ");

        _image = _surface.Snapshot();

        OnComplete?.Invoke();
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

        string path = fileService.GenerateDirectedGraphFilePath();

        OnStart?.Invoke($"Saving image to {path}... ");

        using (SKData data = _image.Encode(SKEncodedImageFormat.Png, 100))
        using (FileStream stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        OnComplete?.Invoke();
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
            Color = ConvertColorToSKColor(node.Shape.Color)
        };

        SKPaint borderPaint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = ConvertColorToSKColor(node.Shape.BorderColor),
            StrokeWidth = 5
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

        ShapeConfiguration shapeConfiguration = node.Shape.GetShapeConfiguration();

        switch (node.Shape.ShapeType)
        {
            case ShapeType.Ellipse:
                DrawEllipse(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            case ShapeType.Polygon:
                DrawPolygon(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            case ShapeType.SemiCircle:
                DrawSemiCircle(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            case ShapeType.Arc:
                DrawArc(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            case ShapeType.Pill:
                DrawPill(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            case ShapeType.Star:
                DrawStar(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            case ShapeType.Seashell:
                DrawSeashell(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            case ShapeType.Trapezoid:
                DrawTrapezoid(canvas, node, shapeConfiguration, paint, borderPaint);
                break;

            default:
                throw new Exception($"No drawing method for ShapeType {node.Shape.ShapeType}");
        }

        if (drawNumbersOnNodes)
        {
            canvas.DrawText(node.NumberValue.ToString(),
                            (float)node.Position.X,
                            (float)node.NumberTextYPosition,
                            textPaint);
        }

        DrawNodeHalo(canvas, node, shapeConfiguration);

        paint.Dispose();
        borderPaint.Dispose();
        textPaint.Dispose();
    }

    /// <summary>
    /// Generate a skew matrix to skew the drawn shape by pre-determined amounts
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="skew"></param>
    /// <returns></returns>
    private static SKMatrix GetSkewSKMatrix((double X, double Y) nodePosition, (double X, double Y) skew)
    {
        SKMatrix skewMatrix = SKMatrix.CreateSkew((float)skew.X,
                                                  (float)skew.Y);

        SKMatrix translateToOrigin = SKMatrix.CreateTranslation(-(float)nodePosition.X, -(float)nodePosition.Y);
        SKMatrix translateBack = SKMatrix.CreateTranslation((float)nodePosition.X, (float)nodePosition.Y);

        return translateToOrigin.PostConcat(skewMatrix)
                                .PostConcat(translateBack);
    }

    /// <summary>
    /// Draw a light halo around nodes, with decreasing intensity as distance from the light source increases
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    private static void DrawNodeHalo(SKCanvas canvas,
                                     DirectedGraphNode node,
                                     ShapeConfiguration shapeConfiguration)
    {
        if (shapeConfiguration.HaloConfiguration == null)
        {
            return;
        }

        SKColor skColor = ConvertColorToSKColor(shapeConfiguration.HaloConfiguration.Value.Color);

        using SKPaint haloPaint = new()
        {
            Shader = SKShader.CreateRadialGradient(ConvertCoordinatesToSKPoint(node.Position),
                                                   (float)shapeConfiguration.HaloConfiguration.Value.Radius,
                                                   new[] { skColor, SKColors.Transparent },
                                                   null,
                                                   SKShaderTileMode.Clamp),
            Style = SKPaintStyle.Fill
        };

        canvas.DrawCircle(ConvertCoordinatesToSKPoint(node.Position),
                          (float)shapeConfiguration.HaloConfiguration.Value.Radius,
                          haloPaint);
    }

    /// <summary>
    /// Draw the lines connecting nodes to their parent/children
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    private static void DrawNodeConnection(SKCanvas canvas,
                                           DirectedGraphNode node)
    {
        using SKPaint paint = new()
        {
            Color = SKColors.White.WithAlpha(100),
            StrokeWidth = 2,
            IsAntialias = true
        };

        foreach (DirectedGraphNode childNode in node.Children)
        {
            canvas.DrawLine(ConvertCoordinatesToSKPoint(node.Position),
                            ConvertCoordinatesToSKPoint(childNode.Position),
                            paint);

            canvas.DrawCircle(ConvertCoordinatesToSKPoint(childNode.Position),
                              (float)childNode.Shape.Radius * 0.10f,
                              paint);
        }
    }

    /// <summary>
    /// Draw a white point on the canvas with a blur effect
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="point"></param>
    private static void DrawStarWithBlur(SKCanvas canvas,
                                         SKPoint point)
    {
        double starSize = Random.Shared.Next(1, 6);
        double blurRadius = starSize * 0.9;

        using SKPaint blurPaint = new()
        {
            IsAntialias = true,
            MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, (float)blurRadius)
        };

        using SKPaint starPaint = new()
        {
            IsAntialias = true,
            Color = SKColors.White
        };

        canvas.DrawCircle(point, (float)starSize, blurPaint);
        canvas.DrawCircle(point, (float)starSize, starPaint);
    }

    /// <summary>
    /// Convert the given x,y coordinates to an SKPoint object
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private static SKPoint ConvertCoordinatesToSKPoint((double X, double Y) position)
    {
        return new SKPoint((float)position.X, (float)position.Y);
    }

    /// <summary>
    /// Get an SKColor object from a Color object
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private static SKColor ConvertColorToSKColor(Color color)
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
        _image?.Dispose();
        _image = null;

        _canvas?.Dispose();
        _canvas = null;

        _surface?.Dispose();
        _surface = null;

        _nodes = null;

        GC.SuppressFinalize(this);
    }

    #endregion
}