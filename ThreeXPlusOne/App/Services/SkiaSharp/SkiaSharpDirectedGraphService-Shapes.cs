using SkiaSharp;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Models.ShapeConfiguration;

namespace ThreeXPlusOne.App.Services.SkiaSharp;

public partial class SkiaSharpDirectedGraphService
{
    private void DrawShapeFromVertices(SKCanvas canvas,
                                       DirectedGraphNode node,
                                       ShapeConfiguration shapeConfiguration,
                                       SKPaint paint,
                                       SKPaint borderPaint)
    {
        if (shapeConfiguration.Vertices == null)
        {
            throw new Exception($"{nameof(DrawShapeFromVertices)}: Vertices were null");
        }

        using SKPath shapePath = new();

        for (int i = 0; i < shapeConfiguration.Vertices.Count; i++)
        {
            (double X, double Y) vertex = shapeConfiguration.Vertices[i];

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

        if (shapeConfiguration.Skew == null)
        {
            Draw2DShape(shapePath,
                        canvas,
                        paint,
                        borderPaint,
                        node,
                        shapeConfiguration);

            return;
        }

        DrawSkewed3DShape(shapePath,
                          canvas,
                          paint,
                          borderPaint,
                          node,
                          shapeConfiguration);
    }

    private void DrawEllipse(SKCanvas canvas,
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
            Draw2DShape(ellipsePath,
                        canvas,
                        paint,
                        borderPaint,
                        node,
                        shapeConfiguration);

            return;
        }

        DrawSkewed3DShape(ellipsePath,
                          canvas,
                          paint,
                          borderPaint,
                          node,
                          shapeConfiguration);
    }

    private void DrawSemiCircle(SKCanvas canvas,
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
            Draw2DShape(semiCirclePath,
                        canvas,
                        paint,
                        borderPaint,
                        node,
                        shapeConfiguration);

            return;
        }

        DrawSkewed3DShape(semiCirclePath,
                          canvas,
                          paint,
                          borderPaint,
                          node,
                          shapeConfiguration);
    }

    private void DrawArc(SKCanvas canvas,
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
            Draw2DShape(arcPath,
                        canvas,
                        paint,
                        borderPaint,
                        node,
                        shapeConfiguration);

            DrawArcBottomBorders(arcPath, canvas, borderPaint);

            return;
        }

        DrawSkewed3DShape(arcPath,
                          canvas,
                          paint,
                          borderPaint,
                          node,
                          shapeConfiguration);
    }

    private void DrawPill(SKCanvas canvas,
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
            Draw2DShape(pillPath,
                        canvas,
                        paint,
                        borderPaint,
                        node,
                        shapeConfiguration);

            return;
        }

        DrawSkewed3DShape(pillPath,
                          canvas,
                          paint,
                          borderPaint,
                          node,
                          shapeConfiguration);
    }

    /// <summary>
    /// Draw a 2D shape with one face
    /// Use a gradient for the colour of the face if the shape is impacted by the light source
    /// </summary>
    /// <param name="path"></param>
    /// <param name="canvas"></param>
    /// <param name="paint"></param>
    /// <param name="borderPaint"></param>
    /// <param name="node"></param>
    /// <param name="shapeConfiguration"></param>
    private static void Draw2DShape(SKPath path,
                                    SKCanvas canvas,
                                    SKPaint paint,
                                    SKPaint borderPaint,
                                    DirectedGraphNode node,
                                    ShapeConfiguration shapeConfiguration)
    {
        if (node.Shape.HasLightSourceImpact)
        {
            SKPoint frontFaceGradientStartPoint = ConvertCoordinatesToSKPoint(shapeConfiguration.FrontFaceGradientStartPoint);
            SKPoint frontFaceGradientEndPoint = ConvertCoordinatesToSKPoint(shapeConfiguration.FrontFaceGradientEndPoint);

            SKColor[] gradientColors = [ConvertColorToSKColor(node.Shape.GradientStartColor),
                                        ConvertColorToSKColor(node.Shape.GradientEndColor)];

            SKColor[] borderGradientColors = [ConvertColorToSKColor(node.Shape.BorderGradientStartColor),
                                              ConvertColorToSKColor(node.Shape.BorderGradientEndColor)];

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

            paint.Shader = frontFaceShader;
            borderPaint.Shader = borderShader;
        }

        canvas.DrawPath(path, paint);
        canvas.DrawPath(path, borderPaint);
    }

    /// <summary>
    /// Draw two offset faces with the same skew and add sides
    /// Use gradients for the colours if the shape is impacted by the light source
    /// </summary>
    /// <param name="path"></param>
    /// <param name="canvas"></param>
    /// <param name="paint"></param>
    /// <param name="borderPaint"></param>
    /// <param name="node"></param>
    /// <param name="shapeConfiguration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private void DrawSkewed3DShape(SKPath path,
                                   SKCanvas canvas,
                                   SKPaint paint,
                                   SKPaint borderPaint,
                                   DirectedGraphNode node,
                                   ShapeConfiguration shapeConfiguration)
    {
        if (shapeConfiguration.Skew == null)
        {
            throw new Exception($"{nameof(DrawSkewed3DShape)}: Skew settings were null");
        }

        double depth = shapeConfiguration.ThreeDimensionalDepth(node.Shape.Radius);

        SKMatrix skewMatrix = GetSkewSKMatrix(node.Position, shapeConfiguration.Skew.Value);

        path.Transform(skewMatrix);

        SKMatrix offsetMatrix = SKMatrix.CreateTranslation((float)depth, -(float)depth);

        SKPath backPath = new(path);
        backPath.Transform(offsetMatrix);

        using SKPaint sidePaint = new()
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        int sidePoints = shapeConfiguration.ThreeDimensionalSideCount;  // Number of points to use for the sides

        SKPoint[] frontPoints = GetPointsOnPath(path, sidePoints);
        SKPoint[] backPoints = GetPointsOnPath(backPath, sidePoints);

        if (node.Shape.HasLightSourceImpact)
        {
            if (_lightSourceCoordinates == null)
            {
                throw new Exception("Node has light source impact but light source coordinates were null");
            }

            SKPoint frontFaceGradientStartPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(shapeConfiguration.FrontFaceGradientStartPoint));
            SKPoint frontFaceGradientEndPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(shapeConfiguration.FrontFaceGradientEndPoint));

            SKPoint sideGradientStartPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(shapeConfiguration.SideFaceGradientStartPoint));
            SKPoint sideGradientEndPoint = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(shapeConfiguration.SideFaceGradientEndPoint));

            SKColor[] gradientColors = [ConvertColorToSKColor(node.Shape.GradientStartColor),
                                        ConvertColorToSKColor(node.Shape.GradientEndColor)];

            SKColor[] borderGradientColors = [ConvertColorToSKColor(node.Shape.BorderGradientStartColor),
                                              ConvertColorToSKColor(node.Shape.BorderGradientEndColor)];

            SKPoint skPointLightSourceCoordinates = ConvertCoordinatesToSKPoint(_lightSourceCoordinates.Value);

            // Calculate the direction of the light source relative to the skewed shape
            SKPoint skewedCenter = skewMatrix.MapPoint(ConvertCoordinatesToSKPoint(node.Position));
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

            paint.Shader = frontFaceShader;
            sidePaint.Shader = sideShader;
            borderPaint.Shader = borderShader;
        }
        else
        {
            sidePaint.Color = paint.Color;
        }

        //draw back face
        canvas.DrawPath(backPath, paint);
        //note: don't draw back face border as it creates a feeling of bluriness when viewed at a distance

        for (int i = 0; i < sidePoints; i++)
        {
            if (node.Shape.HasGap &&
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

            canvas.DrawPath(sidePath, sidePaint);
        }

        //draw front face
        canvas.DrawPath(path, paint);
        canvas.DrawPath(path, borderPaint);

        if (node.Shape.ShapeType == Enums.ShapeType.Arc)
        {
            //front face
            DrawArcBottomBorders(path, canvas, borderPaint);

            //rear face
            DrawArcBottomBorders(backPath, canvas, borderPaint);
        }
    }

    /// <summary>
    /// Get an array of the coordinates of all points around the shape's path
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
    /// Generate a skew matrix to skew the drawn shape by pre-determined amounts
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
    /// Draw the border of the two ends of the arc without drawing across the gap of the arc
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
    /// Calculate the Euclidean distance between two node positions
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns></returns>
    protected static double Distance(SKPoint position1,
                                     SKPoint position2)
    {
        return Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
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
    /// Normalize a point (vector)
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
    /// Calculate the dot product of two points (vectors)
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
    /// Calculate the length of a point (vector)
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private static float Length(SKPoint point)
    {
        return (float)Math.Sqrt(point.X * point.X + point.Y * point.Y);
    }
}
