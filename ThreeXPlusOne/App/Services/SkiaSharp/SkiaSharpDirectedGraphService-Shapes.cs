using SkiaSharp;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.Services.SkiaSharp;

public partial class SkiaSharpDirectedGraphService
{
    private static void DrawEllipse(SKCanvas canvas,
                                    DirectedGraphNode node,
                                    ShapeConfiguration shapeConfiguration,
                                    SKPaint paint,
                                    SKPaint borderPaint)
    {
        if (shapeConfiguration.EllipseConfiguration == null)
        {
            throw new Exception($"{nameof(DrawEllipse)}: Ellipse configuration settings were null");
        }

        using SKPath ellipsePath = new();

        ellipsePath.AddOval(new SKRect((float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Left,
                                       (float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Top,
                                       (float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Right,
                                       (float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Bottom));

        if (shapeConfiguration.Skew == null)
        {
            canvas.DrawPath(ellipsePath, paint);
            canvas.DrawPath(ellipsePath, borderPaint);

            return;
        }

        DrawSpheroid(canvas,
                     node.Position,
                     shapeConfiguration.EllipseConfiguration.RadiusX,
                     shapeConfiguration.EllipseConfiguration.RadiusY,
                     shapeConfiguration.Skew.Value);

        //skewed 3D shape
        /*
        DrawThreeDimensionalDepth(ellipsePath,
                                  canvas,
                                  paint,
                                  borderPaint,
                                  node,
                                  shapeConfiguration.Skew.Value,
                                  true);

                                  */
    }

    public static void DrawSpheroid(SKCanvas canvas, (double X, double Y) position, double width, double height, (double X, double Y) skewPosition)
    {
        // Create an elliptical gradient to simulate lighting on the spheroid
        SKPoint center = new((float)position.X, (float)position.Y);
        SKColor[] colors = [SKColors.White.WithAlpha(200), SKColors.LightGray.WithAlpha(200), SKColors.DarkGray.WithAlpha(200)];
        float[] colorPositions = [0.0f, 0.5f, 1.0f];

        // Define the gradient's transformation matrix to create an elliptical shape
        SKMatrix gradientMatrix = SKMatrix.CreateScale((float)width / (float)height, 1.0f, center.X, center.Y);
        SKShader shader = SKShader.CreateRadialGradient(center, (float)height, colors, colorPositions, SKShaderTileMode.Clamp, gradientMatrix);

        // Create a paint with the gradient shader
        SKPaint paint = new()
        {
            Shader = shader,
            IsAntialias = true
        };

        canvas.Save();

        SKMatrix skewMatrix = GetSkewSKMatrix(position,
                                              skewPosition);

        canvas.Concat(ref skewMatrix);

        // Draw the ellipse representing the spheroid
        canvas.DrawOval((float)center.X - (float)width / 2, (float)center.Y - (float)height / 2, (float)width, (float)height, paint);

        canvas.Restore();
    }

    private static void DrawShapeWithVertices(SKCanvas canvas,
                                              DirectedGraphNode node,
                                              ShapeConfiguration shapeConfiguration,
                                              SKPaint paint,
                                              SKPaint borderPaint)
    {
        if (shapeConfiguration.Vertices == null)
        {
            throw new Exception($"{nameof(DrawShapeWithVertices)}: Vertices were null");
        }

        using SKPath polygonPath = new();

        for (int i = 0; i < shapeConfiguration.Vertices.Count; i++)
        {
            (double X, double Y) vertex = shapeConfiguration.Vertices[i];

            if (i == 0)
            {
                polygonPath.MoveTo(ConvertCoordinatesToSKPoint(vertex));
            }
            else
            {
                polygonPath.LineTo(ConvertCoordinatesToSKPoint(vertex));
            }
        }

        polygonPath.Close();

        if (shapeConfiguration.Skew == null)
        {
            canvas.DrawPath(polygonPath, paint);
            canvas.DrawPath(polygonPath, borderPaint);

            return;
        }

        //skewed 3D shape
        DrawThreeDimensionalDepth(polygonPath,
                                  canvas,
                                  paint,
                                  borderPaint,
                                  node,
                                  shapeConfiguration.Skew.Value,
                                  true);
    }

    private static void DrawSemiCircle(SKCanvas canvas,
                                       DirectedGraphNode node,
                                       ShapeConfiguration shapeConfiguration,
                                       SKPaint paint,
                                       SKPaint borderPaint)
    {
        if (shapeConfiguration.SemiCircleConfiguration == null)
        {
            throw new Exception($"{nameof(DrawSemiCircle)}: SemiCircle configuration settings were null");
        }

        using SKPath semiCirclePath = new();

        semiCirclePath.AddArc(new SKRect((float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Left,
                                         (float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Top,
                                         (float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Right,
                                         (float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Bottom),
                              (float)shapeConfiguration.SemiCircleConfiguration.Orientation,
                              (float)shapeConfiguration.SemiCircleConfiguration.SweepAngle);

        semiCirclePath.Close();

        if (shapeConfiguration.Skew == null)
        {
            canvas.DrawPath(semiCirclePath, paint);
            canvas.DrawPath(semiCirclePath, borderPaint);

            return;
        }

        //skewed 3D shape
        DrawThreeDimensionalDepth(semiCirclePath,
                                  canvas,
                                  paint,
                                  borderPaint,
                                  node,
                                  shapeConfiguration.Skew.Value,
                                  false);
    }

    private static void DrawArc(SKCanvas canvas,
                                DirectedGraphNode node,
                                ShapeConfiguration shapeConfiguration,
                                SKPaint paint,
                                SKPaint borderPaint)
    {
        if (shapeConfiguration.ArcConfiguration == null)
        {
            throw new Exception($"{nameof(DrawArc)}: Arc configuration settings were null");
        }

        using SKPath arcPath = new();

        // Top edge of the arc
        arcPath.AddArc(new SKRect((float)shapeConfiguration.ArcConfiguration.TopArcBounds.Left,
                                  (float)shapeConfiguration.ArcConfiguration.TopArcBounds.Top,
                                  (float)shapeConfiguration.ArcConfiguration.TopArcBounds.Right,
                                  (float)shapeConfiguration.ArcConfiguration.TopArcBounds.Bottom),
                       (float)shapeConfiguration.ArcConfiguration.TopArcStartAngle,
                       (float)shapeConfiguration.ArcConfiguration.TopArcSweepAngle);

        // Bottom edge of the arc (drawn in reverse)
        arcPath.AddArc(new SKRect((float)shapeConfiguration.ArcConfiguration.BottomArcBounds.Left,
                                  (float)shapeConfiguration.ArcConfiguration.BottomArcBounds.Top,
                                  (float)shapeConfiguration.ArcConfiguration.BottomArcBounds.Right,
                                  (float)shapeConfiguration.ArcConfiguration.BottomArcBounds.Bottom),
                       (float)shapeConfiguration.ArcConfiguration.BottomArcStartAngle,
                       (float)shapeConfiguration.ArcConfiguration.BottomArcSweepAngle);

        if (shapeConfiguration.Skew == null)
        {
            canvas.DrawPath(arcPath, paint);
            canvas.DrawPath(arcPath, borderPaint);

            //Draw the border of the two ends of the arc
            // Use SKPathMeasure to get the start and end points of the top and bottom arcs
            SKPathMeasure pathMeasure = new(arcPath, false);
            float topArcLength = pathMeasure.Length;

            // Get the start and end points of the top arc
            pathMeasure.GetPosition(0, out SKPoint topArcStartPoint);
            pathMeasure.GetPosition(topArcLength, out SKPoint topArcEndPoint);

            // Move to the next contour (the bottom arc)
            pathMeasure.NextContour();
            float bottomArcLength = pathMeasure.Length;

            // Get the start and end points of the bottom arc (reversed)
            pathMeasure.GetPosition(0, out SKPoint bottomArcEndPoint);
            pathMeasure.GetPosition(bottomArcLength, out SKPoint bottomArcStartPoint);

            // Draw lines connecting the end points of the arcs
            canvas.DrawLine(topArcStartPoint.X, topArcStartPoint.Y, bottomArcStartPoint.X, bottomArcStartPoint.Y, borderPaint);
            canvas.DrawLine(topArcEndPoint.X, topArcEndPoint.Y, bottomArcEndPoint.X, bottomArcEndPoint.Y, borderPaint);

            return;
        }

        //skewed 3D shape
        SKPath backFacePath = DrawThreeDimensionalDepth(arcPath,
                                                        canvas,
                                                        paint,
                                                        borderPaint,
                                                        node,
                                                        shapeConfiguration.Skew.Value,
                                                        false);

        //Draw the border of the two ends of the arc
        // Use SKPathMeasure to get the start and end points of the top and bottom arcs
        SKPathMeasure pathMeasure1 = new(arcPath, false);
        float topArcLength1 = pathMeasure1.Length;

        // Get the start and end points of the top arc
        pathMeasure1.GetPosition(0, out SKPoint topArcStartPoint1);
        pathMeasure1.GetPosition(topArcLength1, out SKPoint topArcEndPoint1);

        // Move to the next contour (the bottom arc)
        pathMeasure1.NextContour();
        float bottomArcLength1 = pathMeasure1.Length;

        // Get the start and end points of the bottom arc (reversed)
        pathMeasure1.GetPosition(0, out SKPoint bottomArcEndPoint1);
        pathMeasure1.GetPosition(bottomArcLength1, out SKPoint bottomArcStartPoint1);

        // Draw lines connecting the end points of the arcs
        canvas.DrawLine(topArcStartPoint1.X, topArcStartPoint1.Y, bottomArcStartPoint1.X, bottomArcStartPoint1.Y, borderPaint);
        canvas.DrawLine(topArcEndPoint1.X, topArcEndPoint1.Y, bottomArcEndPoint1.X, bottomArcEndPoint1.Y, borderPaint);

        //repeat drawing the bottom of the arc for the back face
        SKPathMeasure pathMeasure2 = new(backFacePath, false);
        float topArcLength2 = pathMeasure2.Length;

        pathMeasure2.GetPosition(0, out SKPoint topArcStartPoint2);
        pathMeasure2.GetPosition(topArcLength2, out SKPoint topArcEndPoint2);

        pathMeasure2.NextContour();
        float bottomArcLength2 = pathMeasure2.Length;

        pathMeasure2.GetPosition(0, out SKPoint bottomArcEndPoint2);
        pathMeasure2.GetPosition(bottomArcLength2, out SKPoint bottomArcStartPoint2);

        canvas.DrawLine(topArcStartPoint2.X, topArcStartPoint2.Y, bottomArcStartPoint2.X, bottomArcStartPoint2.Y, borderPaint);
        canvas.DrawLine(topArcEndPoint2.X, topArcEndPoint2.Y, bottomArcEndPoint2.X, bottomArcEndPoint2.Y, borderPaint);
    }

    private static void DrawPill(SKCanvas canvas,
                                 DirectedGraphNode node,
                                 ShapeConfiguration shapeConfiguration,
                                 SKPaint paint,
                                 SKPaint borderPaint)
    {
        if (shapeConfiguration.PillConfiguration == null)
        {
            throw new Exception($"{nameof(DrawPill)}: Pill configuration settings were null");
        }

        using SKPath pillPath = new();

        SKRect pillBounds = new((float)shapeConfiguration.PillConfiguration.ShapeBounds.Left,
                                (float)shapeConfiguration.PillConfiguration.ShapeBounds.Top,
                                (float)shapeConfiguration.PillConfiguration.ShapeBounds.Right,
                                (float)shapeConfiguration.PillConfiguration.ShapeBounds.Bottom);

        pillPath.AddRoundRect(pillBounds,
                              (float)shapeConfiguration.PillConfiguration.CurveRadiusX,
                              (float)shapeConfiguration.PillConfiguration.CurveRadiusY,
                              SKPathDirection.Clockwise);

        SKMatrix rotationMatrix = SKMatrix.CreateRotationDegrees((float)shapeConfiguration.PillConfiguration.RotationAngle,
                                                                 (float)node.Position.X,
                                                                 (float)node.Position.Y);

        pillPath.Transform(rotationMatrix);

        if (shapeConfiguration.Skew == null)
        {
            canvas.DrawPath(pillPath, paint);
            canvas.DrawPath(pillPath, borderPaint);

            return;
        }

        //skewed 3D shape
        DrawThreeDimensionalDepth(pillPath,
                                  canvas,
                                  paint,
                                  borderPaint,
                                  node,
                                  shapeConfiguration.Skew.Value,
                                  false);
    }

    private static SKPath DrawThreeDimensionalDepth(SKPath path,
                                                SKCanvas canvas,
                                                SKPaint paint,
                                                SKPaint borderPaint,
                                                DirectedGraphNode node,
                                                (double X, double Y) skewPosition,
                                                bool drawSidePath)
    {
        double threeDimensionalDepth = node.Shape.Radius * 0.1;

        path.Transform(GetSkewSKMatrix(node.Position, skewPosition));

        SKPath backFacePath = new(path);
        SKMatrix offsetMatrix = SKMatrix.CreateTranslation((float)threeDimensionalDepth, (float)-threeDimensionalDepth);
        backFacePath.Transform(offsetMatrix);

        canvas.DrawPath(path, paint);
        canvas.DrawPath(backFacePath, paint);

        SKPoint[] frontPoints = path.Points;
        SKPoint[] backPoints = backFacePath.Points;

        if (drawSidePath)
        {
            for (int i = 0; i < path.PointCount; i++)
            {
                SKPath sidePath = new();
                sidePath.MoveTo(frontPoints[i]);
                sidePath.LineTo(backPoints[i]);
                sidePath.LineTo(backPoints[(i + 1) % path.PointCount]);
                sidePath.LineTo(frontPoints[(i + 1) % path.PointCount]);
                sidePath.Close();

                SKPoint startPoint = frontPoints[i];
                SKPoint endPoint = backPoints[i];
                SKColor[] colors = [paint.Color, AdjustColorBrightness(paint.Color, 0.6f)];
                SKShader shader = SKShader.CreateLinearGradient(startPoint, endPoint, colors, null, SKShaderTileMode.Clamp);

                using (SKPaint sidePaint = new() { IsAntialias = true, Style = SKPaintStyle.Fill, Shader = shader })
                {
                    canvas.DrawPath(sidePath, sidePaint);
                }

                canvas.DrawLine(frontPoints[i], backPoints[i], borderPaint);
            }
        }

        canvas.DrawPath(path, borderPaint);
        canvas.DrawPath(backFacePath, borderPaint);

        return backFacePath;
    }

    private static SKColor AdjustColorBrightness(SKColor color, float factor)
    {
        byte r = (byte)Math.Clamp(color.Red * factor, 0, 255);
        byte g = (byte)Math.Clamp(color.Green * factor, 0, 255);
        byte b = (byte)Math.Clamp(color.Blue * factor, 0, 255);

        return new SKColor(r, g, b).WithAlpha(color.Alpha);
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
}