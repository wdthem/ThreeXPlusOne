using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public class HistogramService(IHistogramDrawingService histogramDrawingService,
                              IFileService fileService,
                              IConsoleService consoleService) : IHistogramService
{
    /// <summary>
    /// Generate the histogram based on the lists of series data.
    /// </summary>
    /// <param name="seriesData"></param>
    public async Task GenerateHistogram(List<List<int>> seriesData)
    {
        consoleService.WriteHeading("Histogram");
        consoleService.Write("Generating histogram... ");

        string filePath = fileService.GenerateHistogramFilePath();

        if (fileService.FileExists(filePath))
        {
            consoleService.WriteLine("already exists\n");

            return;
        }

        List<int> digitCounts = GenerateHistogramData(seriesData);

        int width = 500;
        int height = 400;

        histogramDrawingService.Initialize(width, height);
        histogramDrawingService.Draw(digitCounts, "Numbers in series starting from 1-9");
        await histogramDrawingService.SaveImage(filePath);
        histogramDrawingService.Dispose();

        consoleService.WriteDone();
    }

    /// <summary>
    /// Create separate lists for numbers starting from 1 to 9.
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
    private static List<int> GenerateHistogramData(List<List<int>> seriesData)
    {
        List<int> digitCounts = Enumerable.Repeat(0, 9).ToList();

        foreach (List<int> list in seriesData)
        {
            foreach (int number in list)
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
}