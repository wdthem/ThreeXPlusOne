using SkiaSharp;
using System.Collections.ObjectModel;
using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services.SkiaSharp;

public class SkiaSharpDirectedGraphService(IFileService fileService) : IDirectedGraphService
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

            points.Add(ConvertCoordinatesToSKPoint(x, y));
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
        SKShader shader = SKShader.CreateRadialGradient(ConvertCoordinatesToSKPoint(lightSourceCoordinates.X, lightSourceCoordinates.Y),
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

        DrawShape(canvas,
                  node,
                  paint,
                  borderPaint);

        if (drawNumbersOnNodes)
        {
            // Draw the text
            // Adjust the Y coordinate to account for text height (this centers the text vertically in the circle)
            double textY = node.Position.Y + 8;

            canvas.DrawText(node.NumberValue.ToString(),
                            (float)node.Position.X,
                            (float)textY,
                            textPaint);
        }

        paint.Dispose();
        textPaint.Dispose();
    }

    /// <summary>
    /// Draw either a circle or a polygon for the node based on app settings
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    /// <param name="paint"></param>
    /// <param name="borderPaint"></param>
    private static void DrawShape(SKCanvas canvas,
                                  DirectedGraphNode node,
                                  SKPaint paint,
                                  SKPaint borderPaint)
    {
        ShapeConfiguration shapeConfiguration = node.Shape.GetShapeConfiguration();

        if (node.Shape.ShapeType == ShapeType.Ellipse)
        {
            canvas.DrawOval((float)shapeConfiguration.EllipseConfig.Center.X,
                            (float)shapeConfiguration.EllipseConfig.Center.Y,
                            (float)shapeConfiguration.EllipseConfig.RadiusX,
                            (float)shapeConfiguration.EllipseConfig.RadiusY,
                            paint);

            canvas.DrawOval((float)shapeConfiguration.EllipseConfig.Center.X,
                            (float)shapeConfiguration.EllipseConfig.Center.Y,
                            (float)shapeConfiguration.EllipseConfig.RadiusX,
                            (float)shapeConfiguration.EllipseConfig.RadiusY,
                            borderPaint);
        }

        if (node.Shape.ShapeType == ShapeType.SemiCircle)
        {
            using SKPath semiCirclePath = new();

            semiCirclePath.AddArc(new SKRect((float)node.Position.X - (float)node.Shape.Radius,
                                             (float)node.Position.Y - (float)node.Shape.Radius,
                                             (float)node.Position.X + (float)node.Shape.Radius,
                                             (float)node.Position.Y + (float)node.Shape.Radius),
                                  (float)shapeConfiguration.SemiCircleOrientation,
                                  180);

            semiCirclePath.Close();

            canvas.DrawPath(semiCirclePath, paint);
            canvas.DrawPath(semiCirclePath, borderPaint);
        }

        if (node.Shape.ShapeType == ShapeType.Arc)
        {
            float innerRadius = (float)node.Shape.Radius - (float)shapeConfiguration.ArcConfig.Thickness / 2;
            float outerRadius = (float)node.Shape.Radius + (float)shapeConfiguration.ArcConfig.Thickness / 2;

            using SKPath arcPath = new();

            // Top edge of the rainbow
            arcPath.AddArc(new SKRect((float)node.Position.X - outerRadius,
                                      (float)node.Position.Y - outerRadius,
                                      (float)node.Position.X + outerRadius,
                                      (float)node.Position.Y + outerRadius),
                           (float)shapeConfiguration.ArcConfig.StartAngle,
                           (float)shapeConfiguration.ArcConfig.SweepAngle);

            // Bottom edge of the rainbow (drawn in reverse)
            arcPath.AddArc(new SKRect((float)node.Position.X - innerRadius,
                                      (float)node.Position.Y - innerRadius,
                                      (float)node.Position.X + innerRadius,
                                      (float)node.Position.Y + innerRadius),
                           (float)shapeConfiguration.ArcConfig.StartAngle + (float)shapeConfiguration.ArcConfig.SweepAngle,
                           (float)-shapeConfiguration.ArcConfig.SweepAngle);

            canvas.DrawPath(arcPath, paint);
            canvas.DrawPath(arcPath, borderPaint);
        }

        if (node.Shape.ShapeType == ShapeType.Pill)
        {
            using SKPath pillPath = new();

            float pillWidth = (float)node.Shape.Radius;
            float pillHeight = (float)shapeConfiguration.PillConfig.Height;

            SKRect pillRect = new((float)node.Position.X - pillWidth / 2,
                                  (float)node.Position.Y - pillHeight / 2,
                                  (float)node.Position.X + pillWidth / 2,
                                  (float)node.Position.Y + pillHeight / 2);

            pillPath.AddRoundRect(pillRect, pillHeight / 2, pillHeight / 2, SKPathDirection.Clockwise);

            SKMatrix rotationMatrix = SKMatrix.CreateRotationDegrees((float)shapeConfiguration.PillConfig.RotationAngle,
                                                                     (float)node.Position.X,
                                                                     (float)node.Position.Y);

            pillPath.Transform(rotationMatrix);

            if (shapeConfiguration.PillConfig.Skew.X > 0 &&
                shapeConfiguration.PillConfig.Skew.Y > 0)
            {
                SKMatrix skewMatrix = SKMatrix.CreateSkew((float)shapeConfiguration.PillConfig.Skew.X,
                                                          (float)shapeConfiguration.PillConfig.Skew.Y);

                SKMatrix translateToOrigin = SKMatrix.CreateTranslation(-(float)node.Position.X, -(float)node.Position.Y);
                SKMatrix translateBack = SKMatrix.CreateTranslation((float)node.Position.X, (float)node.Position.Y);
                SKMatrix combinedMatrix = translateToOrigin.PostConcat(skewMatrix).PostConcat(translateBack);

                pillPath.Transform(combinedMatrix);
            }

            canvas.DrawPath(pillPath, paint);
            canvas.DrawPath(pillPath, borderPaint);
        }

        if (node.Shape.ShapeType == ShapeType.Polygon)
        {
            using SKPath polygonPath = new();

            for (int i = 0; i < shapeConfiguration.PolygonVertices.Count; i++)
            {
                (double x, double y) = shapeConfiguration.PolygonVertices[i];

                if (i == 0)
                {
                    polygonPath.MoveTo(ConvertCoordinatesToSKPoint(x, y));
                }
                else
                {
                    polygonPath.LineTo(ConvertCoordinatesToSKPoint(x, y));
                }
            }

            polygonPath.Close();
            canvas.DrawPath(polygonPath, paint);
            canvas.DrawPath(polygonPath, borderPaint);
        }

        RenderNodeHaloEffect(canvas, node);
    }

    /// <summary>
    /// Render a light halo around nodes, with decreasing intensity as distance from the light source increases
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    private static void RenderNodeHaloEffect(SKCanvas canvas,
                                             DirectedGraphNode node)
    {
        if (node.Shape.HaloConfig.Color == Color.Empty)
        {
            return;
        }

        SKColor skColor = ConvertColorToSKColor(node.Shape.HaloConfig.Color);

        using SKPaint haloPaint = new()
        {
            Shader = SKShader.CreateRadialGradient(ConvertCoordinatesToSKPoint(node.Position.X, node.Position.Y),
                                                   (float)node.Shape.HaloConfig.Radius,
                                                   new[] { skColor, SKColors.Transparent },
                                                   null,
                                                   SKShaderTileMode.Clamp),
            Style = SKPaintStyle.Fill
        };

        canvas.DrawCircle(ConvertCoordinatesToSKPoint(node.Position.X, node.Position.Y),
                          (float)node.Shape.HaloConfig.Radius,
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
            canvas.DrawLine(ConvertCoordinatesToSKPoint(node.Position.X, node.Position.Y),
                            ConvertCoordinatesToSKPoint(childNode.Position.X, childNode.Position.Y),
                            paint);

            canvas.DrawCircle(ConvertCoordinatesToSKPoint(childNode.Position.X, childNode.Position.Y),
                              (float)childNode.Shape.Radius * 0.10f,
                              paint);
        }
    }

    /// <summary>
    /// Apply a blur effect to the stars
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="point"></param>
    private static void DrawStarWithBlur(SKCanvas canvas,
                                         SKPoint point)
    {
        double starSize = Random.Shared.Next(5, 16);
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
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static SKPoint ConvertCoordinatesToSKPoint(double x, double y)
    {
        return new SKPoint((float)x, (float)y);
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
