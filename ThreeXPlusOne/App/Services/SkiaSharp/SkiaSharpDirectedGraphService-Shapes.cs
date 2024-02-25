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
            throw new Exception("DrawEllipse: Ellipse configuration settings were null");
        }

        using SKPath ellipsePath = new();

        ellipsePath.AddOval(new SKRect((float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Left,
                                       (float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Top,
                                       (float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Right,
                                       (float)shapeConfiguration.EllipseConfiguration.ShapeBounds.Bottom));

        if (shapeConfiguration.Skew != null)
        {
            ellipsePath.Transform(GetSkewSKMatrix(node.Position,
                                                  shapeConfiguration.Skew.Value));
        }

        canvas.DrawPath(ellipsePath, paint);
        canvas.DrawPath(ellipsePath, borderPaint);
    }

    private static void DrawPolygon(SKCanvas canvas,
                                    DirectedGraphNode node,
                                    ShapeConfiguration shapeConfiguration,
                                    SKPaint paint,
                                    SKPaint borderPaint)
    {
        if (shapeConfiguration.PolygonConfiguration == null)
        {
            throw new Exception("DrawPolygon: Polygon configuration settings were null");
        }

        using SKPath polygonPath = new();

        for (int i = 0; i < shapeConfiguration.PolygonConfiguration.Vertices.Count; i++)
        {
            (double X, double Y) vertex = shapeConfiguration.PolygonConfiguration.Vertices[i];

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

        if (shapeConfiguration.Skew != null)
        {
            polygonPath.Transform(GetSkewSKMatrix(node.Position,
                                                  shapeConfiguration.Skew.Value));
        }

        canvas.DrawPath(polygonPath, paint);
        canvas.DrawPath(polygonPath, borderPaint);
    }

    private static void DrawSemiCircle(SKCanvas canvas,
                                       DirectedGraphNode node,
                                       ShapeConfiguration shapeConfiguration,
                                       SKPaint paint,
                                       SKPaint borderPaint)
    {
        if (shapeConfiguration.SemiCircleConfiguration == null)
        {
            throw new Exception("DrawSemiCircle: SemiCircle configuration settings were null");
        }

        using SKPath semiCirclePath = new();

        semiCirclePath.AddArc(new SKRect((float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Left,
                                         (float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Top,
                                         (float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Right,
                                         (float)shapeConfiguration.SemiCircleConfiguration.ShapeBounds.Bottom),
                              (float)shapeConfiguration.SemiCircleConfiguration.Orientation,
                              (float)shapeConfiguration.SemiCircleConfiguration.SweepAngle);

        semiCirclePath.Close();

        if (shapeConfiguration.Skew != null)
        {
            semiCirclePath.Transform(GetSkewSKMatrix(node.Position,
                                                     shapeConfiguration.Skew.Value));
        }

        canvas.DrawPath(semiCirclePath, paint);
        canvas.DrawPath(semiCirclePath, borderPaint);
    }

    private static void DrawArc(SKCanvas canvas,
                                DirectedGraphNode node,
                                ShapeConfiguration shapeConfiguration,
                                SKPaint paint,
                                SKPaint borderPaint)
    {
        if (shapeConfiguration.ArcConfiguration == null)
        {
            throw new Exception("DrawArc: Arc configuration settings were null");
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

        if (shapeConfiguration.Skew != null)
        {
            arcPath.Transform(GetSkewSKMatrix(node.Position, shapeConfiguration.Skew.Value));
        }

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
    }

    private static void DrawPill(SKCanvas canvas,
                                 DirectedGraphNode node,
                                 ShapeConfiguration shapeConfiguration,
                                 SKPaint paint,
                                 SKPaint borderPaint)
    {
        if (shapeConfiguration.PillConfiguration == null)
        {
            throw new Exception("DrawPill: Pill configuration settings were null");
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

        if (shapeConfiguration.Skew != null)
        {
            pillPath.Transform(GetSkewSKMatrix(node.Position,
                                               shapeConfiguration.Skew.Value));
        }

        canvas.DrawPath(pillPath, paint);
        canvas.DrawPath(pillPath, borderPaint);
    }

    private static void DrawStar(SKCanvas canvas,
                                 DirectedGraphNode node,
                                 ShapeConfiguration shapeConfiguration,
                                 SKPaint paint,
                                 SKPaint borderPaint)
    {
        if (shapeConfiguration.StarConfiguration == null)
        {
            throw new Exception("DrawStar: Star configuration settings were null");
        }

        using SKPath starPath = new();

        for (int lcv = 0; lcv < shapeConfiguration.StarConfiguration.AngleVertices.Count; lcv++)
        {
            SKPoint point = ConvertCoordinatesToSKPoint(shapeConfiguration.StarConfiguration.AngleVertices[lcv]);

            if (lcv == 0)
            {
                starPath.MoveTo(point);
            }
            else
            {
                starPath.LineTo(point);
            }
        }

        starPath.Close();

        if (shapeConfiguration.Skew != null)
        {
            starPath.Transform(GetSkewSKMatrix(node.Position,
                                               shapeConfiguration.Skew.Value));
        }

        canvas.DrawPath(starPath, paint);
        canvas.DrawPath(starPath, borderPaint);
    }

    private static void DrawSeashell(SKCanvas canvas,
                                     DirectedGraphNode node,
                                     ShapeConfiguration shapeConfiguration,
                                     SKPaint paint,
                                     SKPaint borderPaint)
    {
        if (shapeConfiguration.SeashellConfiguration == null)
        {
            throw new Exception("DrawSeashell: Seashell configuration settings were null");
        }

        using SKPath seashellPath = new();

        seashellPath.MoveTo(ConvertCoordinatesToSKPoint(node.Position));

        foreach ((double x, double y) coordinate in shapeConfiguration.SeashellConfiguration.SpiralCoordinates)
        {
            seashellPath.LineTo(ConvertCoordinatesToSKPoint(coordinate));
        }

        seashellPath.Close();

        if (shapeConfiguration.Skew != null)
        {
            seashellPath.Transform(GetSkewSKMatrix(node.Position,
                                                   shapeConfiguration.Skew.Value));
        }

        canvas.DrawPath(seashellPath, paint);
        canvas.DrawPath(seashellPath, borderPaint);
    }

    private static void DrawTrapezoid(SKCanvas canvas,
                                      DirectedGraphNode node,
                                      ShapeConfiguration shapeConfiguration,
                                      SKPaint paint,
                                      SKPaint borderPaint)
    {
        if (shapeConfiguration.TrapezoidConfiguration == null)
        {
            throw new Exception("DrawTrapezoid: Trapezoid configuration settings were null");
        }

        using SKPath trapezoidPath = new();

        trapezoidPath.MoveTo(ConvertCoordinatesToSKPoint(shapeConfiguration.TrapezoidConfiguration.TopLeftVertex));
        trapezoidPath.LineTo(ConvertCoordinatesToSKPoint(shapeConfiguration.TrapezoidConfiguration.TopRightVertex));
        trapezoidPath.LineTo(ConvertCoordinatesToSKPoint(shapeConfiguration.TrapezoidConfiguration.BottomRightVertex));
        trapezoidPath.LineTo(ConvertCoordinatesToSKPoint(shapeConfiguration.TrapezoidConfiguration.BottomLeftVertex));

        trapezoidPath.Close();

        if (shapeConfiguration.Skew != null)
        {
            trapezoidPath.Transform(GetSkewSKMatrix(node.Position,
                                                    shapeConfiguration.Skew.Value));
        }

        canvas.DrawPath(trapezoidPath, paint);
        canvas.DrawPath(trapezoidPath, borderPaint);
    }
}