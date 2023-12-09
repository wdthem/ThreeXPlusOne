using SkiaSharp;

namespace ThreeXPlusOne.Code;

internal static class Histogram
{

    internal static void GenerateHistogram(List<List<int>> seriesData, string imagePath)
    {
        int width = 500;
        int height = 400;
        using SKBitmap bitmap = new(width, height);
        using SKCanvas canvas = new(bitmap);
        canvas.Clear(SKColors.White);

        var digitCounts = GenerateHistogramData(seriesData);

        DrawHistogram(canvas, digitCounts, width, height);

        using SKImage image = SKImage.FromBitmap(bitmap);
        using SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite($"{imagePath}histogram.png");

        data.SaveTo(stream);
    }

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

    private static void DrawHistogram(SKCanvas canvas, List<int> counts, int canvasWidth, int canvasHeight)
    {
        int numberOfBars = counts.Count;
        int spacing = 10; // Spacing between bars
        int totalSpacing = spacing * (numberOfBars - 1); // Total spacing for all gaps

        // Reserve space for Y-axis and X-axis labels
        int yAxisLabelSpace = 50; // Adjust this as needed
        int xAxisLabelHeight = 20; // Space for X-axis labels
        int topPadding = 30; // Additional space at the top

        int barWidth = (canvasWidth - totalSpacing - yAxisLabelSpace) / numberOfBars;
        int maxCount = counts.Count > 0 ? counts.Max() : 1;

        // Scale the bars to leave space for the count text
        int adjustedMaxCount = maxCount + (maxCount / 10); // Adjust for space above the tallest bar
        float scaleFactor = (float)(canvasHeight - xAxisLabelHeight - topPadding) / adjustedMaxCount;

        // Determine the number of Y-axis labels (every 500 units)
        int maxYAxisValue = ((adjustedMaxCount + 499) / 500) * 500;
        int yAxisLabels = maxYAxisValue / 500;

        // Draw Y-axis labels and horizontal lines
        for (int i = 0; i <= yAxisLabels; i++)
        {
            int labelValue = i * 500;
            if (labelValue <= adjustedMaxCount) // Ensure label doesn't exceed the adjusted max
            {
                float y = (canvasHeight - xAxisLabelHeight - topPadding) - (labelValue * scaleFactor);

                canvas.DrawLine(yAxisLabelSpace, y, canvasWidth, y, new SKPaint { Color = SKColors.Gray, IsAntialias = true, StrokeWidth = 1 });
                canvas.DrawText(
                    labelValue.ToString(),
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
            SKRect bar = new SKRect(
                yAxisLabelSpace + i * barWidth + i * spacing, // Adjust position for spacing and Y-axis labels
                (canvasHeight - xAxisLabelHeight - topPadding) - barHeight,
                yAxisLabelSpace + (i + 1) * barWidth + i * spacing, // Adjust position for spacing and Y-axis labels
                canvasHeight - xAxisLabelHeight);

            canvas.DrawRect(bar, new SKPaint { Color = SKColors.Blue, IsAntialias = true });

            // Draw X-axis labels
            canvas.DrawText(
                (i + 1).ToString(),
                yAxisLabelSpace + i * barWidth + i * spacing + barWidth / 2, // Adjust position for spacing and Y-axis labels
                canvasHeight - 5, // Position below the graph
                new SKPaint { Color = SKColors.Black, IsAntialias = true, TextAlign = SKTextAlign.Center });

            // Draw bar totals inside the bars near the top
            SKPaint textPaint = new SKPaint { Color = SKColors.White, IsAntialias = true, TextAlign = SKTextAlign.Center };
            float textYPosition = (canvasHeight - xAxisLabelHeight - topPadding) - barHeight + 15; // Adjust for the height of the bar
            if (barHeight > 30) // Check if there's enough space to draw inside the bar
            {
                canvas.DrawText(
                    count.ToString(),
                    yAxisLabelSpace + i * barWidth + i * spacing + barWidth / 2, // Position within the bar
                    textYPosition,
                    textPaint);
            }
        }
    }
}