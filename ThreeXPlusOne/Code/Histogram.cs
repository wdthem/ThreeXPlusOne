using Microsoft.Extensions.Options;
using SkiaSharp;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Histogram(IOptions<Settings> settings,
                       IFileHelper fileHelper,
                       IConsoleHelper consoleHelper) : IHistogram
{
    private readonly Settings _settings = settings.Value;

    public void GenerateHistogram(List<List<int>> seriesData)
    {
        consoleHelper.WriteHeading("Histogram");

        if (_settings.GenerateHistogram)
        {
            consoleHelper.Write("Generating histogram... ");
        }
        else
        {
            consoleHelper.WriteLine("Histogram generation disabled\n");

            return;
        }

        string filePath = fileHelper.GenerateHistogramFilePath();

        if (fileHelper.FileExists(filePath))
        {
            consoleHelper.WriteLine("already exists\n");

            return;
        }

        int width = 500;
        int height = 400;

        using SKBitmap bitmap = new(width, height);
        using SKCanvas canvas = new(bitmap);

        canvas.Clear(SKColors.White);

        List<int> digitCounts = GenerateHistogramData(seriesData);

        DrawHistogram(canvas, digitCounts, width, height, "Numbers in series starting from 1-9");

        SaveHistogram(bitmap, filePath);

        consoleHelper.WriteDone();
    }

    /// <summary>
    /// Create separate lists for numbers starting from 1 to 9
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
    private static List<int> GenerateHistogramData(List<List<int>> seriesData)
    {
        List<int> digitCounts = Enumerable.Repeat(0, 9).ToList();

        foreach (var list in seriesData)
        {
            foreach (var number in list)
            {
                string numberStr = number.ToString();

                int firstDigit = int.Parse(numberStr[0].ToString());

                if (firstDigit >= 1 && firstDigit <= 9)
                {
                    digitCounts[firstDigit - 1]++;
                }
            }
        }

        return digitCounts;
    }

    /// <summary>
    /// Draw the histogram image
    /// </summary>
    /// <param name="canvas"></param>
    /// <param name="counts"></param>
    /// <param name="canvasWidth"></param>
    /// <param name="canvasHeight"></param>
    /// <param name="title"></param>
    private static void DrawHistogram(SKCanvas canvas, List<int> counts, int canvasWidth, int canvasHeight, string title)
    {
        int numberOfBars = counts.Count;
        int spacing = 10; // Spacing between bars
        int totalSpacing = spacing * (numberOfBars - 1); // Total spacing for all gaps

        int yAxisLabelSpace = 50;
        int xAxisLabelHeight = 20;
        int titleHeight = 30;
        int topPadding = 30;

        int effectiveCanvasHeight = canvasHeight - titleHeight; // Adjust height for title space

        int barWidth = (canvasWidth - totalSpacing - yAxisLabelSpace) / numberOfBars;
        int maxCount = counts.Count > 0 ? counts.Max() : 1;

        // Scale the bars to leave space for the count text
        int adjustedMaxCount = maxCount + (maxCount / 10); // Adjust for space above the tallest bar
        float scaleFactor = (float)(effectiveCanvasHeight - xAxisLabelHeight - topPadding) / adjustedMaxCount;

        // Determine the number of Y-axis labels (every 500 units)
        int maxYAxisValue = (adjustedMaxCount + 499) / 500 * 500;
        int yAxisLabels = maxYAxisValue / 500;

        // Draw Y-axis labels and horizontal lines
        for (int i = 0; i <= yAxisLabels; i++)
        {
            int labelValue = i * 500;
            if (labelValue <= adjustedMaxCount) // Ensure label doesn't exceed the adjusted max
            {
                float y = effectiveCanvasHeight - xAxisLabelHeight - topPadding - (labelValue * scaleFactor);

                canvas.DrawLine(yAxisLabelSpace, y, canvasWidth, y, new SKPaint { Color = SKColors.Gray, IsAntialias = true, StrokeWidth = 1 });
                canvas.DrawText(labelValue.ToString(),
                                 yAxisLabelSpace - 10, // Position to the left of the first bar
                                 y,
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
                             effectiveCanvasHeight - xAxisLabelHeight);

            canvas.DrawRect(bar, new SKPaint { Color = SKColors.Blue, IsAntialias = true });

            // Draw X-axis labels
            canvas.DrawText((i + 1).ToString(),
                            yAxisLabelSpace + i * barWidth + i * spacing + barWidth / 2, // Adjust position for spacing and Y-axis labels
                            effectiveCanvasHeight - 5, // Position below the graph
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
                canvas.DrawText(count.ToString(),
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

        canvas.DrawText(title,
                        canvasWidth / 2,
                        canvasHeight - 10, // Position at the bottom
                        titlePaint);
    }

    /// <summary>
    /// Save the generated histogram image
    /// </summary>
    /// <param name="bitmap"></param>
    /// <param name="filePath"></param>
    private static void SaveHistogram(SKBitmap bitmap, string filePath)
    {
        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);

        using var stream = File.OpenWrite(filePath);

        data.SaveTo(stream);
    }
}