using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.App;

public class Histogram(IOptions<Settings> settings,
                       IHistogramService histogramService,
                       IFileService fileService,
                       IConsoleService consoleService) : IHistogram
{
    private readonly Settings _settings = settings.Value;

    public void GenerateHistogram(List<List<int>> seriesData)
    {
        consoleService.WriteHeading("Histogram");

        if (_settings.GenerateHistogram)
        {
            consoleService.Write("Generating histogram... ");
        }
        else
        {
            consoleService.WriteLine("Histogram generation disabled\n");

            return;
        }

        string filePath = fileService.GenerateHistogramFilePath();

        if (fileService.FileExists(filePath))
        {
            consoleService.WriteLine("already exists\n");

            return;
        }

        List<int> digitCounts = GenerateHistogramData(seriesData);

        int width = 500;
        int height = 400;

        histogramService.Initialize(width, height);
        histogramService.Draw(digitCounts, "Numbers in series starting from 1-9");
        histogramService.SaveImage(filePath);
        histogramService.Dispose();

        consoleService.WriteDone();
    }

    /// <summary>
    /// Create separate lists for numbers starting from 1 to 9
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
