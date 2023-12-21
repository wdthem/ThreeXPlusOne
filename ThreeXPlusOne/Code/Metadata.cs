using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Metadata(IOptions<Settings> settings,
                      IFileHelper fileHelper) : IMetadata
{
    public void GenerateMedatadataFile(List<List<int>> seriesData)
    {
        ConsoleOutput.WriteHeading("Metadata");

        if (settings.Value.GenerateMetadataFile)
        {
            Console.Write("Generating metadata... ");

            GenerateNumberSeriesMetadata(seriesData);
            GenerateTop10LongestSeriesMetadata(seriesData);
            GenerateFullSeriesData(seriesData);

            ConsoleOutput.WriteDone();
        }
        else
        {
            Console.WriteLine("Metadata generation disabled");
        }
    }

    private static List<(int FirstNumber, int Count)> GenerateTop10Series(List<List<int>> series)
    {
        return series.Where(list => list.Count != 0).Select(list => (list.First(), list.Count))
                                                    .OrderByDescending(item => item.Count)
                                                    .Take(10)
                                                    .ToList();
    }

    private void GenerateNumberSeriesMetadata(List<List<int>> seriesData)
    {
        var filePath = fileHelper.GenerateMetadataFilePath();

        string content = "\nSeries run for the following numbers: \n";

        var lcv = 1;

        foreach (List<int> values in seriesData)
        {
            if (lcv % 20 == 0)
            {
                content += "\n";
            }

            content += $"{values[0]}, ";

            lcv++;
        }

        fileHelper.WriteMetadataToFile(content, filePath);
    }

    private void GenerateTop10LongestSeriesMetadata(List<List<int>> seriesData)
    {
        var filePath = fileHelper.GenerateMetadataFilePath();

        string content = "\nTop 10 longest series:\n";

        foreach ((int FirstNumber, int Count) in GenerateTop10Series(seriesData))
        {
            content += $"{FirstNumber}: {Count} in series\n";
        }

        fileHelper.WriteMetadataToFile(content, filePath);
    }

    private void GenerateFullSeriesData(List<List<int>> seriesData)
    {
        var filePath = fileHelper.GenerateMetadataFilePath();

        string content = "\nFull series data:\n";

        foreach (List<int> series in seriesData)
        {
            content += string.Join(", ", series);
            content += "\n\n";
        }

        fileHelper.WriteMetadataToFile(content, filePath);
    }
}