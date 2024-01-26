using SkiaSharp;
using ThreeXPlusOne.Code.Enums;
using ThreeXPlusOne.Code.Interfaces;

namespace ThreeXPlusOne.Code.Services;

public class SkiaSharpHistogramService() : IHistogramService
{
    private SKBitmap? _bitmap;
    private SKCanvas? _canvas;
    private int _canvasWidth;
    private int _canvasHeight;

    public GraphProvider GraphProvider => GraphProvider.SkiaSharp;

    /// <summary>
    /// Initialize the histogram
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    public void Initialize(int width, int height)
    {
        _canvasWidth = width;
        _canvasHeight = height;

        _bitmap = new SKBitmap(width, height);
        _canvas = new SKCanvas(_bitmap);

        _canvas.Clear(SKColors.White);
    }

    /// <summary>
    /// Draw the histogram image
    /// </summary>
    /// <param name="counts"></param>
    /// <param name="title"></param>
    public void Draw(List<int> counts,
                     string title)
    {
        if (_canvas == null)
        {
            throw new Exception("Could not draw the histogram. Canvas object was null");
        }

        int numberOfBars = counts.Count;
        int spacing = 10; // Spacing between bars
        int totalSpacing = spacing * (numberOfBars - 1); // Total spacing for all gaps

        int yAxisLabelSpace = 50;
        int xAxisLabelHeight = 20;
        int titleHeight = 30;
        int topPadding = 30;

        int effectiveCanvasHeight = _canvasHeight - titleHeight; // Adjust height for title space

        int barWidth = (_canvasWidth - totalSpacing - yAxisLabelSpace) / numberOfBars;
        int maxCount = counts.Count > 0 ? counts.Max() : 1;

        // Scale the bars to leave space for the count text
        int adjustedMaxCount = maxCount + (maxCount / 10); // Adjust for space above the tallest bar
        double scaleFactor = (effectiveCanvasHeight - xAxisLabelHeight - topPadding) / adjustedMaxCount;

        // Define maximum height and segment count
        const int maxSegmentCount = 10;

        // Define maximum height and a suitable set of segment sizes
        int[] possibleSegmentSizes = [10, 100, 200, 500, 1000, 2000, 5000, 10000];

        // Select the most suitable segment size
        int segmentSize = possibleSegmentSizes[0];
        foreach (int size in possibleSegmentSizes)
        {
            if (adjustedMaxCount / size <= maxSegmentCount)
            {
                segmentSize = size;
                break;
            }
        }

        // Calculate maxYAxisValue based on the selected segment size
        int maxYAxisValue = (adjustedMaxCount + segmentSize - 1) / segmentSize * segmentSize;
        int yAxisLabels = maxYAxisValue / segmentSize;

        // Draw Y-axis labels and horizontal lines
        for (int i = 0; i <= yAxisLabels; i++)
        {
            int labelValue = i * segmentSize;
            if (labelValue <= adjustedMaxCount) // Ensure label doesn't exceed the adjusted max
            {
                double y = effectiveCanvasHeight - xAxisLabelHeight - topPadding - (labelValue * scaleFactor);

                _canvas.DrawLine(yAxisLabelSpace,
                                 (float)y,
                                 _canvasWidth,
                                 (float)y,
                                 new SKPaint { Color = SKColors.Gray, IsAntialias = true, StrokeWidth = 1 });

                _canvas.DrawText(labelValue.ToString(),
                                 yAxisLabelSpace - 10,
                                 (float)y,
                                 new SKPaint { Color = SKColors.Black, IsAntialias = true, TextAlign = SKTextAlign.Right });
            }
        }

        // Draw bars and X-axis labels
        for (int i = 0; i < numberOfBars; i++)
        {
            int count = counts[i];
            int barHeight = (int)(count * scaleFactor);

            SKRect bar = new(yAxisLabelSpace + i * barWidth + i * spacing, // Adjust position for spacing and Y-axis labels
                             effectiveCanvasHeight - xAxisLabelHeight - topPadding - barHeight,
                             yAxisLabelSpace + (i + 1) * barWidth + i * spacing, // Adjust position for spacing and Y-axis labels
                             effectiveCanvasHeight - xAxisLabelHeight - 30); // removing an extra 30 pixels starts the bar at y-axis of 0

            _canvas.DrawRect(bar, new SKPaint { Color = SKColors.Blue, IsAntialias = true });

            // Draw X-axis labels
            _canvas.DrawText((i + 1).ToString(),
                             yAxisLabelSpace + i * barWidth + i * spacing + barWidth / 2, // Adjust position for spacing and Y-axis labels
                             effectiveCanvasHeight - 30, // Position below the graph
                             new SKPaint { Color = SKColors.Black, IsAntialias = true, TextAlign = SKTextAlign.Center });

            // Draw bar totals inside the bars near the top
            SKPaint textPaint = new()
            {
                Color = SKColors.White,
                IsAntialias = true,
                TextAlign = SKTextAlign.Center
            };

            float textYPosition = effectiveCanvasHeight - xAxisLabelHeight - topPadding - barHeight + 15; // Adjust for the height of the bar

            if (barHeight > 30) // Check if there's enough space to draw inside the bar
            {
                _canvas.DrawText(count.ToString(),
                                 yAxisLabelSpace + i * barWidth + i * spacing + barWidth / 2, // Position within the bar
                                 textYPosition,
                                 textPaint);
            }
        }

        // Draw the title below the X-axis labels
        SKPaint titlePaint = new()
        {
            Color = SKColors.Black,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center,
            TextSize = 20
        };

        _canvas.DrawText(title,
                         _canvasWidth / 2,
                         _canvasHeight - 10, // Position at the bottom
                         titlePaint);
    }

    /// <summary>
    /// Save the generated histogram image
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="filePath"></param>
    public void SaveImage(string filePath)
    {
        if (_bitmap == null)
        {
            throw new Exception("Could not save the histogram image. Bitmap object was null");
        }

        using SKImage image = SKImage.FromBitmap(_bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var stream = File.OpenWrite(filePath);

        data.SaveTo(stream);
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
            _canvas?.Dispose();
            _canvas = null;

            _bitmap?.Dispose();
            _bitmap = null;
        }
    }

    /// <summary>
    /// IDisposable finalizer
    /// </summary>
    ~SkiaSharpHistogramService()
    {
        Dispose(false);
    }

    #endregion
}
