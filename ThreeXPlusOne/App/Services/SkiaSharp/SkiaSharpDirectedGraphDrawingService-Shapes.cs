using SkiaSharp;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.DirectedGraph.Interfaces;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services.SkiaSharp;

public partial class SkiaSharpDirectedGraphDrawingService
{
    /// <summary>
    /// Draw an arc shape with the defined settings.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <exception cref="Exception"></exception>
    private void DrawArc(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext)
    {
        if (skiaSharpShapeRenderContext.Node.Shape is not Arc arc)
        {
            throw new ApplicationException($"{nameof(DrawArc)}: Expected shape type not received)");
        }

        using SKPath arcPath = new();

        // Top edge of the arc
        arcPath.AddArc(new SKRect((float)arc.TopArcBounds.Left,
                                  (float)arc.TopArcBounds.Top,
                                  (float)arc.TopArcBounds.Right,
                                  (float)arc.TopArcBounds.Bottom),
                       (float)arc.TopArcStartAngle,
                       (float)arc.TopArcSweepAngle);

        // Bottom edge of the arc (drawn in reverse)
        arcPath.AddArc(new SKRect((float)arc.BottomArcBounds.Left,
                                  (float)arc.BottomArcBounds.Top,
                                  (float)arc.BottomArcBounds.Right,
                                  (float)arc.BottomArcBounds.Bottom),
                       (float)arc.BottomArcStartAngle,
                       (float)arc.BottomArcSweepAngle);

        DrawShape(skiaSharpShapeRenderContext, arcPath);
    }

    /// <summary>
    /// Draw a donut shape with the defined settings.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <exception cref="Exception"></exception>
    private void DrawDonut(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext)
    {
        if (skiaSharpShapeRenderContext.Node.Shape is not Donut donut)
        {
            throw new ApplicationException($"{nameof(DrawDonut)}: Expected shape type not received)");
        }

        using SKPath donutPath = new();
        using SKPath innerPath = new();
        using SKPath reversedInnerPath = new();

        SKRect outerBounds = new((float)donut.OuterEllipseBounds.Left,
                                 (float)donut.OuterEllipseBounds.Top,
                                 (float)donut.OuterEllipseBounds.Right,
                                 (float)donut.OuterEllipseBounds.Bottom);

        SKRect innerBounds = new((float)donut.InnerEllipseBounds.Left,
                                 (float)donut.InnerEllipseBounds.Top,
                                 (float)donut.InnerEllipseBounds.Right,
                                 (float)donut.InnerEllipseBounds.Bottom);

        donutPath.AddOval(outerBounds);

        innerPath.MoveTo(innerBounds.Left, innerBounds.Top);
        innerPath.AddOval(innerBounds);

        reversedInnerPath.AddPathReverse(innerPath);

        donutPath.AddPath(reversedInnerPath);

        DrawShape(skiaSharpShapeRenderContext, donutPath);
    }

    /// <summary>
    /// Draw an ellipse shape with the defined settings.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <exception cref="Exception"></exception>
    private void DrawEllipse(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext)
    {
        if (skiaSharpShapeRenderContext.Node.Shape is not Ellipse ellipse)
        {
            throw new ApplicationException($"{nameof(DrawEllipse)}: Expected shape type not received)");
        }

        using SKPath ellipsePath = new();

        ellipsePath.AddOval(new SKRect((float)ellipse.Bounds.Left,
                                       (float)ellipse.Bounds.Top,
                                       (float)ellipse.Bounds.Right,
                                       (float)ellipse.Bounds.Bottom));

        DrawShape(skiaSharpShapeRenderContext, ellipsePath);
    }

    /// <summary>
    /// Draw a pill shape with the defined settings.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <exception cref="Exception"></exception>
    private void DrawPill(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext)
    {
        if (skiaSharpShapeRenderContext.Node.Shape is not Pill pill)
        {
            throw new ApplicationException($"{nameof(DrawPill)}: Expected shape type not received)");
        }

        using SKPath pillPath = new();

        SKRect pillBounds = new((float)pill.Bounds.Left,
                                (float)pill.Bounds.Top,
                                (float)pill.Bounds.Right,
                                (float)pill.Bounds.Bottom);

        pillPath.AddRoundRect(pillBounds,
                              (float)pill.CurveRadiusX,
                              (float)pill.CurveRadiusY,
                              SKPathDirection.Clockwise);

        SKMatrix rotationMatrix = SKMatrix.CreateRotationDegrees((float)pill.RotationAngle,
                                                                 (float)skiaSharpShapeRenderContext.Node.Position.X,
                                                                 (float)skiaSharpShapeRenderContext.Node.Position.Y);

        pillPath.Transform(rotationMatrix);

        DrawShape(skiaSharpShapeRenderContext, pillPath);
    }

    /// <summary>
    /// Draw a semicircle shape with the defined settings.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <exception cref="Exception"></exception>
    private void DrawSemiCircle(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext)
    {
        if (skiaSharpShapeRenderContext.Node.Shape is not SemiCircle semiCircle)
        {
            throw new ApplicationException($"{nameof(DrawSemiCircle)}: Expected shape type not received)");
        }

        using SKPath semiCirclePath = new();

        semiCirclePath.AddArc(new SKRect((float)semiCircle.Bounds.Left,
                                         (float)semiCircle.Bounds.Top,
                                         (float)semiCircle.Bounds.Right,
                                         (float)semiCircle.Bounds.Bottom),
                              (float)semiCircle.Orientation,
                              (float)semiCircle.SweepAngle);

        semiCirclePath.Close();

        DrawShape(skiaSharpShapeRenderContext, semiCirclePath);
    }

    /// <summary>
    /// Draw a shape based on the defined set of vertices.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <exception cref="Exception"></exception>
    private void DrawShapeFromVertices(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext)
    {
        if (skiaSharpShapeRenderContext.Node.Shape is not IVertexShape vertexShape || vertexShape.Vertices == null)
        {
            throw new ApplicationException($"{nameof(DrawShapeFromVertices)}: Vertices were null");
        }

        using SKPath shapePath = new();

        for (int i = 0; i < vertexShape.Vertices.Count; i++)
        {
            (double X, double Y) vertex = vertexShape.Vertices[i];

            if (i == 0)
            {
                shapePath.MoveTo(ConvertCoordinatesToSKPoint(vertex));
            }
            else
            {
                shapePath.LineTo(ConvertCoordinatesToSKPoint(vertex));
            }
        }

        shapePath.Close();

        DrawShape(skiaSharpShapeRenderContext, shapePath);
    }

    /// <summary>
    /// Draw the shape based on the number of dimensions in which it is being rendered.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <param name="shapePath"></param>
    private void DrawShape(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext,
                           SKPath shapePath)
    {
        switch (skiaSharpShapeRenderContext.Node.Shape.Dimensions)
        {
            case 2:
                Draw2DShape(skiaSharpShapeRenderContext, shapePath);
                break;

            case 3:
                Draw3DShape(skiaSharpShapeRenderContext, shapePath);
                break;

            default:
                throw new ApplicationException("Invalid shape dimensions");
        }
    }

    /// <summary>
    /// Draw a 2D shape with one face.
    /// Use a gradient for the colour of the face if the shape is impacted by the light source.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <param name="shapePath"></param>
    private static void Draw2DShape(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext,
                                    SKPath shapePath)
    {
        if (skiaSharpShapeRenderContext.Node.Shape.HasLightSourceImpact)
        {
            SKPoint frontFaceGradientStartPoint = ConvertCoordinatesToSKPoint(skiaSharpShapeRenderContext.Node.Shape.FrontFaceGradientStartPoint);
            SKPoint frontFaceGradientEndPoint = ConvertCoordinatesToSKPoint(skiaSharpShapeRenderContext.Node.Shape.FrontFaceGradientEndPoint);

            SKColor[] gradientColors = [ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.GradientStartColor),
                                        ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.GradientEndColor)];

            SKColor[] borderGradientColors = [ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.BorderGradientStartColor),
                                              ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.BorderGradientEndColor)];

            SKShader frontFaceShader = SKShader.CreateLinearGradient(frontFaceGradientStartPoint,
                                                                     frontFaceGradientEndPoint,
                                                                     gradientColors,
                                                                     null,
                                                                     SKShaderTileMode.Clamp);

            SKShader borderShader = SKShader.CreateLinearGradient(frontFaceGradientStartPoint,
                                                                  frontFaceGradientEndPoint,
                                                                  borderGradientColors,
                                                                  null,
                                                                  SKShaderTileMode.Clamp);

            skiaSharpShapeRenderContext.Paint.Shader = frontFaceShader;
            skiaSharpShapeRenderContext.BorderPaint.Shader = borderShader;
        }

        skiaSharpShapeRenderContext.Canvas.DrawPath(shapePath, skiaSharpShapeRenderContext.Paint);
        skiaSharpShapeRenderContext.Canvas.DrawPath(shapePath, skiaSharpShapeRenderContext.BorderPaint);

        if (skiaSharpShapeRenderContext.Node.Shape.ShapeType == Enums.ShapeType.Arc)
        {
            DrawArcBottomBorders(shapePath, skiaSharpShapeRenderContext.Canvas, skiaSharpShapeRenderContext.BorderPaint);
        }
    }

    /// <summary>
    /// Draw two off-set faces with the same skew and add sides.
    /// Use gradients for the colours if the shape is impacted by the light source.
    /// </summary>
    /// <param name="skiaSharpShapeRenderContext"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private void Draw3DShape(SkiaSharpShapeRenderContext skiaSharpShapeRenderContext,
                             SKPath path)
    {
        if (skiaSharpShapeRenderContext.Node.Shape.Skew == null)
        {
            throw new ApplicationException($"{nameof(Draw3DShape)}: Skew settings were null");
        }

        double depth = skiaSharpShapeRenderContext.Node.Shape.ThreeDimensionalDepth(skiaSharpShapeRenderContext.Node.Shape.Radius);

        SKMatrix skewMatrix = GetSkewSKMatrix(skiaSharpShapeRenderContext.Node.Position, skiaSharpShapeRenderContext.Node.Shape.Skew.Value);

        path.Transform(skewMatrix);

        SKMatrix offsetMatrix = SKMatrix.CreateTranslation((float)depth, -(float)depth);

        SKPath backPath = new(path);
        backPath.Transform(offsetMatrix);

        using SKPaint sidePaint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        int sidePoints = skiaSharpShapeRenderContext.Node.Shape.ThreeDimensionalSideCount;  // Number of points to use for the sides. More means less aliasing.

        SKPoint[] frontPoints = GetPointsOnPath(path, sidePoints);
        SKPoint[] backPoints = GetPointsOnPath(backPath, sidePoints);

        if (skiaSharpShapeRenderContext.Node.Shape.HasLightSourceImpact)
        {
            if (_lightSourceCoordinates == null)
            {
                throw new ApplicationException("Node has light source impact but light source coordinates were null");
            }

            SKPoint frontFaceGradientStartPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(skiaSharpShapeRenderContext.Node.Shape.FrontFaceGradientStartPoint));
            SKPoint frontFaceGradientEndPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(skiaSharpShapeRenderContext.Node.Shape.FrontFaceGradientEndPoint));

            SKPoint sideGradientStartPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(skiaSharpShapeRenderContext.Node.Shape.SideFaceGradientStartPoint));
            SKPoint sideGradientEndPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(skiaSharpShapeRenderContext.Node.Shape.SideFaceGradientEndPoint));

            SKColor[] gradientColors = [ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.GradientStartColor),
                                        ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.GradientEndColor)];

            SKColor[] borderGradientColors = [ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.BorderGradientStartColor),
                                              ConvertColorToSKColor(skiaSharpShapeRenderContext.Node.Shape.BorderGradientEndColor)];

            SKPoint skPointLightSourceCoordinates = ConvertCoordinatesToSKPoint(_lightSourceCoordinates.Value);

            // Calculate the direction of the light source relative to the skewed shape
            SKPoint skewedCenter = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(skiaSharpShapeRenderContext.Node.Position));
            SKPoint lightDirection = skPointLightSourceCoordinates - skewedCenter;

            // Project the gradient start and end points onto the light direction vector
            SKPoint projectedStartPoint = ProjectPointOntoVector(frontFaceGradientStartPoint, lightDirection, skewedCenter);
            SKPoint projectedEndPoint = ProjectPointOntoVector(frontFaceGradientEndPoint, lightDirection, skewedCenter);

            frontFaceGradientStartPoint = projectedStartPoint;
            frontFaceGradientEndPoint = projectedEndPoint;
            sideGradientStartPoint = projectedStartPoint;
            sideGradientEndPoint = projectedEndPoint;

            SKShader frontFaceShader = SKShader.CreateLinearGradient(frontFaceGradientStartPoint,
                                                                     frontFaceGradientEndPoint,
                                                                     gradientColors,
                                                                     null,
                                                                     SKShaderTileMode.Clamp);

            SKShader sideShader = SKShader.CreateLinearGradient(sideGradientStartPoint,
                                                                sideGradientEndPoint,
                                                                gradientColors,
                                                                null,
                                                                SKShaderTileMode.Clamp);

            SKShader borderShader = SKShader.CreateLinearGradient(frontFaceGradientStartPoint,
                                                                  frontFaceGradientEndPoint,
                                                                  borderGradientColors,
                                                                  null,
                                                                  SKShaderTileMode.Clamp);

            skiaSharpShapeRenderContext.Paint.Shader = frontFaceShader;
            sidePaint.Shader = sideShader;
            skiaSharpShapeRenderContext.BorderPaint.Shader = borderShader;
        }
        else
        {
            sidePaint.Color = skiaSharpShapeRenderContext.Paint.Color;
        }

        //draw back face
        skiaSharpShapeRenderContext.Canvas.DrawPath(backPath, skiaSharpShapeRenderContext.Paint);
        //note: don't draw back face border as it creates a feeling of bluriness when viewed at a distance

        for (int i = 0; i < sidePoints; i++)
        {
            if (skiaSharpShapeRenderContext.Node.Shape.HasGap &&
                i == sidePoints - 1)
            {
                // Skip the last side for shapes with a gap
                continue;
            }

            using SKPath sidePath = new();

            sidePath.MoveTo(frontPoints[i]);
            sidePath.LineTo(backPoints[i]);
            sidePath.LineTo(backPoints[(i + 1) % sidePoints]);
            sidePath.LineTo(frontPoints[(i + 1) % sidePoints]);

            sidePath.Close();

            skiaSharpShapeRenderContext.Canvas.DrawPath(sidePath, sidePaint);
        }

        //draw front face
        skiaSharpShapeRenderContext.Canvas.DrawPath(path, skiaSharpShapeRenderContext.Paint);
        skiaSharpShapeRenderContext.Canvas.DrawPath(path, skiaSharpShapeRenderContext.BorderPaint);

        if (skiaSharpShapeRenderContext.Node.Shape.ShapeType == Enums.ShapeType.Arc)
        {
            //front face
            DrawArcBottomBorders(path, skiaSharpShapeRenderContext.Canvas, skiaSharpShapeRenderContext.BorderPaint);

            //rear face
            DrawArcBottomBorders(backPath, skiaSharpShapeRenderContext.Canvas, skiaSharpShapeRenderContext.BorderPaint);
        }
    }

    /// <summary>
    /// Get an array of the coordinates of all points around the shape's path.
    /// </summary>
    /// <param name="path"></param>
    /// <param name="pointCount"></param>
    /// <returns></returns>
    private static SKPoint[] GetPointsOnPath(SKPath path,
                                             int pointCount)
    {
        SKPoint[] points = new SKPoint[pointCount];

        using SKPathMeasure pathMeasure = new(path, false);

        float length = pathMeasure.Length;
        float distance = 0;
        float interval = length / (pointCount - 1);

        for (int i = 0; i < pointCount; i++)
        {
            pathMeasure.GetPosition(distance, out points[i]);
            distance += interval;
        }

        return points;
    }

    /// <summary>
    /// Generate a skew matrix to skew the drawn shape by pre-determined amounts.
    /// </summary>
    /// <param name="nodePosition"></param>
    /// <param name="skew"></param>
    /// <returns></returns>
    private static SKMatrix GetSkewSKMatrix((double X, double Y) nodePosition,
                                            (double X, double Y) skew)
    {
        SKMatrix skewMatrix = SKMatrix.CreateSkew((float)skew.X,
                                                  (float)skew.Y);

        SKMatrix translateToOrigin = SKMatrix.CreateTranslation(-(float)nodePosition.X,
                                                                -(float)nodePosition.Y);

        SKMatrix translateBack = SKMatrix.CreateTranslation((float)nodePosition.X,
                                                            (float)nodePosition.Y);

        return translateToOrigin.PostConcat(skewMatrix)
                                .PostConcat(translateBack);
    }

    /// <summary>
    /// Draw the border of the two ends of the arc without drawing across the gap of the arc.
    /// </summary>
    /// <param name="arcPath"></param>
    /// <param name="canvas"></param>
    /// <param name="borderPaint"></param>
    private static void DrawArcBottomBorders(SKPath arcPath,
                                             SKCanvas canvas,
                                             SKPaint borderPaint)
    {
        // Get the start and end points of the top and bottom arcs
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
        canvas.DrawLine(topArcStartPoint.X,
                        topArcStartPoint.Y,
                        bottomArcStartPoint.X,
                        bottomArcStartPoint.Y,
                        borderPaint);

        canvas.DrawLine(topArcEndPoint.X,
                        topArcEndPoint.Y,
                        bottomArcEndPoint.X,
                        bottomArcEndPoint.Y,
                        borderPaint);
    }

    /// <summary>
    /// Projects a point onto a vector originating from a specified origin point.
    /// </summary>
    /// <param name="point">The point to be projected onto the vector.</param>
    /// <param name="vector">The vector onto which the point is projected. The vector is defined relative to the origin.</param>
    /// <param name="origin">The origin point from which the vector originates.</param>
    /// <returns>The projected point on the vector.</returns>
    /// <remarks>
    /// This method calculates the projection of a point onto a vector, effectively
    /// finding the closest point on the vector to the original point. The projection
    /// is performed in the direction of the vector, starting from the origin point.
    /// </remarks>
    private static SKPoint ProjectPointOntoVector(SKPoint point,
                                                  SKPoint vector,
                                                  SKPoint origin)
    {
        SKPoint relativePoint = point - origin;
        float projectionLength = DotProduct(relativePoint, vector) / Length(vector);
        SKPoint normalizedVector = Normalize(vector);

        return new SKPoint(origin.X + normalizedVector.X * projectionLength,
                           origin.Y + normalizedVector.Y * projectionLength);
    }

    /// <summary>
    /// Normalize a point (vector).
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private static SKPoint Normalize(SKPoint point)
    {
        float length = Length(point);

        return new SKPoint(point.X / length,
                           point.Y / length);
    }

    /// <summary>
    /// Calculate the dot product of two points (vectors).
    /// </summary>
    /// <param name="point1"></param>
    /// <param name="point2"></param>
    /// <returns></returns>
    private static float DotProduct(SKPoint point1,
                                    SKPoint point2)
    {
        return point1.X * point2.X + point1.Y * point2.Y;
    }

    /// <summary>
    /// Calculate the length of a point (vector).
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private static float Length(SKPoint point)
    {
        return (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
    }
}