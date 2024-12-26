using SkiaSharp;
using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Enums.Extensions;
using ThreeXPlusOne.App.Helpers;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Services.SkiaSharp;

public partial class SkiaSharpDirectedGraphDrawingService(IFileService fileService) : IDirectedGraphDrawingService
{
    private List<DirectedGraphNode>? _nodes;
    private SKBitmap? _bitmap;
    private SKCanvas? _canvas;
    private (double X, double Y)? _lightSourceCoordinates;

    public Action<string>? OnStart { get; set; }
    public Action<string?>? OnComplete { get; set; }
    public GraphProvider GraphProvider => GraphProvider.SkiaSharp;

    /// <summary>
    /// Initialise SKBitmap and SKCanvas objects based on the provided dimensions.
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
        OnStart?.Invoke($"  Initialising {GraphProvider} graph... ");

        _nodes = nodes;
        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);

        _canvas.Clear(ConvertColorToSKColor(backgroundColor));

        OnComplete?.Invoke(null);
    }

    /// <summary>
    /// Add a light source.
    /// </summary>
    /// <param name="lightSourceCoordinates"></param>
    /// <param name="radius"></param>
    /// <param name="color"></param>
    /// <exception cref="Exception"></exception>
    public void GenerateLightSource((double X, double Y) lightSourceCoordinates,
                                    double radius,
                                    Color color)
    {
        OnStart?.Invoke("  Generating light source... ");

        if (_canvas == null)
        {
            throw new ApplicationException("Could not add light source. Canvas object was null.");
        }

        _lightSourceCoordinates = lightSourceCoordinates;

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

        OnComplete?.Invoke(null);
    }

    /// <summary>
    /// Draw the graph.
    /// </summary>
    /// <param name="drawNumbersOnNodes"></param>
    /// <param name="drawNodeConnections"></param>
    /// <exception cref="Exception"></exception>
    public void Draw(bool drawNumbersOnNodes,
                     bool drawNodeConnections)
    {
        if (_canvas == null || _nodes == null)
        {
            throw new ApplicationException("Could not draw the graph. Canvas object or Nodes object was null");
        }

        string extraMessage = "";

        if (drawNodeConnections)
        {
            extraMessage = " and connections";
        }

        OnStart?.Invoke($"  Drawing {_nodes.Count} nodes{extraMessage}... ");

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

        OnComplete?.Invoke(null);
    }

    /// <summary>
    /// Save the generated graph as the file type specified in the app settings.
    /// </summary>
    /// <param name="imageTypeAppSetting"></param>
    /// <param name="imageQualityAppSetting"></param>
    /// <exception cref="Exception"></exception>
    public void SaveImage(string imageTypeAppSetting,
                          int imageQualityAppSetting)
    {
        OnStart?.Invoke("  Saving image... ");

        if (_bitmap == null)
        {
            throw new ApplicationException("Could not save graph. Bitmap object was null");
        }

        ImageType imageType = ParseImageType(imageTypeAppSetting);

        if (imageQualityAppSetting < 1 || imageQualityAppSetting > 100)
        {
            throw new ApplicationException($"Invalid image quality {imageQualityAppSetting}. Quality must be between 1 and 100.");
        }

        string path = fileService.GenerateDirectedGraphFilePath(imageType);

        using (SKImage image = SKImage.FromBitmap(_bitmap))
        using (SKData data = imageType switch
        {
            ImageType.Png => image.Encode(SKEncodedImageFormat.Png, imageQualityAppSetting),
            ImageType.Jpeg => image.Encode(SKEncodedImageFormat.Jpeg, imageQualityAppSetting),
            ImageType.Webp => image.Encode(SKEncodedImageFormat.Webp, imageQualityAppSetting),
            _ => throw new ArgumentException($"Unsupported image type: {imageType}")
        })
        using (FileStream stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        string ansiFileLink = HrefHelper.GetLocalFileLink(path, $"[IcyBlue]Open {imageType} file[/]");

        OnComplete?.Invoke($"{Emoji.Picture.GetUnicodeValue()} {ansiFileLink}");
    }

    /// <summary>
    /// Parse the value from appSettings into an ImageType enum value.
    /// </summary>
    /// <param name="appSettingsValue"></param>
    /// <returns></returns>
    private static ImageType ParseImageType(string appSettingsValue)
    {
        if (!Enum.TryParse(appSettingsValue, out ImageType imageType))
        {
            throw new ApplicationException($"Invalid image type {appSettingsValue}");
        }

        return imageType;
    }

    /// <summary>
    /// Draw an individual node.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    /// <param name="drawNumbersOnNodes"></param>
    private void DrawNode(SKCanvas canvas,
                          DirectedGraphNode node,
                          bool drawNumbersOnNodes)
    {
        using SKPaint paint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill,
            Color = ConvertColorToSKColor(node.Shape.Color)
        };

        using SKPaint borderPaint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            Color = ConvertColorToSKColor(node.Shape.BorderColor),
            StrokeWidth = 5,
            StrokeJoin = SKStrokeJoin.Bevel
        };

        SkiaSharpShapeRenderContext skiaSharpShapeRenderContext = new()
        {
            Canvas = canvas,
            Node = node,
            Paint = paint,
            BorderPaint = borderPaint
        };

        switch (node.Shape.ShapeType)
        {
            case ShapeType.Arc:
                DrawArc(skiaSharpShapeRenderContext);
                break;

            case ShapeType.Donut:
                DrawDonut(skiaSharpShapeRenderContext);
                break;

            case ShapeType.Ellipse:
                DrawEllipse(skiaSharpShapeRenderContext);
                break;

            case ShapeType.Pill:
                DrawPill(skiaSharpShapeRenderContext);
                break;

            case ShapeType.SemiCircle:
                DrawSemiCircle(skiaSharpShapeRenderContext);
                break;

            case ShapeType.Plus:
            case ShapeType.Polygon:
            case ShapeType.Seashell:
            case ShapeType.Star:
                DrawShapeFromVertices(skiaSharpShapeRenderContext);
                break;

            default:
                throw new ApplicationException($"No drawing method for ShapeType {node.Shape.ShapeType}");
        }

        if (drawNumbersOnNodes)
        {
            using SKPaint textPaint = new()
            {
                Color = SKColors.White,
                IsAntialias = true,
                Style = SKPaintStyle.Fill,
                TextAlign = SKTextAlign.Center,
                TextSize = 20,
                FakeBoldText = true,
            };

            canvas.DrawText(node.NumberValue.ToString(),
                            (float)node.Position.X,
                            (float)node.NumberTextYPosition,
                            textPaint);
        }
    }

    /// <summary>
    /// Draw a Bezier curve connecting two nodes, terminating with a circle head.
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="node"></param>
    private static void DrawNodeConnection(SKCanvas canvas, DirectedGraphNode node)
    {
        foreach (DirectedGraphNode childNode in node.Children)
        {
            SKShader? shader = null;

            if (node.Shape.BorderColor != childNode.Shape.BorderColor)
            {
                shader = SKShader.CreateLinearGradient(ConvertCoordinatesToSKPoint(node.Position),
                                                       ConvertCoordinatesToSKPoint(childNode.Position),
                                                       [
                                                         ConvertColorToSKColor(node.Shape.BorderColor),
                                                         ConvertColorToSKColor(node.Shape.BorderColor).WithAlpha(128),
                                                         ConvertColorToSKColor(childNode.Shape.BorderColor),
                                                         ConvertColorToSKColor(childNode.Shape.BorderColor).WithAlpha(128)
                                                       ],
                                                       [0.0f, 0.45f, 0.55f, 1.0f],
                                                       SKShaderTileMode.Clamp);
            }

            using SKPaint paint = new()
            {
                StrokeWidth = 3,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            };

            if (shader != null)
            {
                paint.Shader = shader;
            }
            else
            {
                paint.Color = ConvertColorToSKColor(node.Shape.BorderColor);
            }

            SKPoint startPoint = ConvertCoordinatesToSKPoint(node.Position);
            SKPoint endPoint = ConvertCoordinatesToSKPoint(childNode.Position);

            // Calculate the distance between the nodes
            float distance = (float)Math.Sqrt(Math.Pow(endPoint.X - startPoint.X, 2) + Math.Pow(endPoint.Y - startPoint.Y, 2));

            // Set a threshold for minimal distance before adjusting curvature
            float minDistanceForCurve = (float)(node.Shape.Radius + childNode.Shape.Radius) * 1.5f;

            SKPoint controlPoint;

            // If nodes are extremely close, use a straight line
            if (distance < minDistanceForCurve / 2)
            {
                canvas.DrawLine(startPoint, endPoint, paint);
            }
            else
            {
                // Check if a spiral center is available for consistent curvature
                if (node is SpiralDirectedGraphNode spiralNode)
                {
                    // Calculate consistent outward curvature relative to spiral center
                    SKPoint spiralCenterPoint = ConvertCoordinatesToSKPoint(spiralNode.SpiralCenter);
                    SKPoint midPoint = ConvertCoordinatesToSKPoint(((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2));

                    SKPoint directionVector = ConvertCoordinatesToSKPoint((midPoint.X - spiralCenterPoint.X, midPoint.Y - spiralCenterPoint.Y));

                    // Normalize the direction vector
                    float magnitude = (float)Math.Sqrt(directionVector.X * directionVector.X + directionVector.Y * directionVector.Y);
                    SKPoint normalizedDirection = ConvertCoordinatesToSKPoint((directionVector.X / magnitude, directionVector.Y / magnitude));

                    // Apply exponential scaling to the control point offset based on distance from spiral center
                    float maxControlPointOffset = 150; // Increase max offset to allow more curvature for outer nodes
                    float controlPointOffset;

                    // Calculate the distance from the spiral center
                    float distanceFromSpiralCenter = (float)Math.Sqrt(Math.Pow(midPoint.X - spiralCenterPoint.X, 2) + Math.Pow(midPoint.Y - spiralCenterPoint.Y, 2));
                    float exponent = distanceFromSpiralCenter > 5000 ? 0.5f : 1.2f;

                    controlPointOffset = Math.Min((float)Math.Pow(distance / 10, exponent), maxControlPointOffset);

                    controlPoint = new SKPoint(midPoint.X + normalizedDirection.X * controlPointOffset, midPoint.Y + normalizedDirection.Y * controlPointOffset);
                }
                else
                {
                    // Default curve behavior for non-spiral graphs
                    SKPoint midPoint = ConvertCoordinatesToSKPoint(((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2));

                    if (distance < minDistanceForCurve)
                    {
                        float reducedOffset = distance / 4;
                        controlPoint = ConvertCoordinatesToSKPoint((midPoint.X, midPoint.Y - reducedOffset));
                    }
                    else
                    {
                        // Apply exponential scaling for distant nodes (without spiral center)
                        float maxControlPointOffset = 50; // Allow more curvature for outer nodes
                        float controlPointOffset = Math.Min((float)Math.Pow(distance / 10, 1.2), maxControlPointOffset);
                        controlPoint = ConvertCoordinatesToSKPoint((midPoint.X, midPoint.Y - controlPointOffset));
                    }
                }

                using SKPath nodeConnectorPath = new();

                nodeConnectorPath.MoveTo(startPoint);
                nodeConnectorPath.QuadTo(controlPoint, endPoint);

                // Draw the Bezier curve path
                canvas.DrawPath(nodeConnectorPath, paint);
            }

            // Optionally draw a small circle at the child node position for visual connection
            canvas.DrawCircle(endPoint, (float)childNode.Shape.Radius * 0.10f, paint);
        }
    }

    /// <summary>
    /// Convert the given x,y coordinates to an SKPoint object.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    private static SKPoint ConvertCoordinatesToSKPoint((double X, double Y) position)
    {
        return new SKPoint((float)position.X, (float)position.Y);
    }

    /// <summary>
    /// Get an SKColor object from a Color object.
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
    /// IDisposable Dispose method.
    /// </summary>
    public void Dispose()
    {
        _canvas?.Dispose();
        _canvas = null;

        _bitmap?.Dispose();
        _bitmap = null;

        _nodes = null;

        GC.SuppressFinalize(this);
    }

    #endregion
}