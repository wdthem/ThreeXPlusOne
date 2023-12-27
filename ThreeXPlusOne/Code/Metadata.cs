using System.Text;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Metadata(IOptions<Settings> settings,
                      IFileHelper fileHelper,
                      IConsoleHelper consoleHelper) : IMetadata
{
    private readonly Settings _settings = settings.Value;

    public void GenerateMedatadataFile(List<List<int>> seriesData)
    {
        consoleHelper.WriteHeading("Metadata");

        if (_settings.GenerateMetadataFile)
        {
            consoleHelper.Write("Generating metadata... ");

            string filePath = fileHelper.GenerateMetadataFilePath();

            if (fileHelper.FileExists(filePath))
            {
                consoleHelper.WriteLine("already exists\n");

                return;
            }

            StringBuilder content = new();

            content.Append(GenerateNumberSeriesMetadata(seriesData));
            content.Append(GenerateTop10LongestSeriesMetadata(seriesData));
            content.Append(GenerateFullSeriesData(seriesData));

            fileHelper.WriteMetadataToFile(content.ToString(), filePath);

            consoleHelper.WriteDone();
        }
        else
        {
            consoleHelper.WriteLine("Metadata generation disabled\n");
        }
    }

    private static List<(int FirstNumber, int Count)> GenerateTop10Series(List<List<int>> series)
    {
        return series.Where(list => list.Count != 0).Select(list => (list.First(), list.Count))
                                                    .OrderByDescending(item => item.Count)
                                                    .Take(10)
                                                    .ToList();
    }

    private static string GenerateNumberSeriesMetadata(List<List<int>> seriesData)
    {
        StringBuilder content = new("\nSeries run for the following numbers: \n");

        int lcv = 1;

        foreach (List<int> values in seriesData)
        {
            content.Append($"{values[0]}, ");

            lcv++;
        }

        return content.ToString();
    }

    private static string GenerateTop10LongestSeriesMetadata(List<List<int>> seriesData)
    {
        StringBuilder content = new("\n\nTop 10 longest series:\n");

        foreach ((int FirstNumber, int Count) in GenerateTop10Series(seriesData))
        {
            content.Append($"{FirstNumber}: {Count} in series\n");
        }

        return content.ToString();
    }

    private static string GenerateFullSeriesData(List<List<int>> seriesData)
    {
        StringBuilder content = new("\nFull series data:\n");

        foreach (List<int> series in seriesData)
        {
            content.Append(string.Join(", ", series));
            content.Append("\n\n");
        }

        return content.ToString();
    }
}