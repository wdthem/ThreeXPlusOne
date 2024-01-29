using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Code.Interfaces.Helpers;
using ThreeXPlusOne.Code.Interfaces.Services;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Histogram(IOptions<Settings> settings,
                       IHistogramService histogramService,
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

        List<int> digitCounts = GenerateHistogramData(seriesData);

        int width = 500;
        int height = 400;

        histogramService.Initialize(width, height);
        histogramService.Draw(digitCounts, "Numbers in series starting from 1-9");
        histogramService.SaveImage(filePath);
        histogramService.Dispose();

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
