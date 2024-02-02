using Microsoft.Extensions.Options;
using System.Text;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App;

public class Metadata(IOptions<Settings> settings,
                      IFileService fileService,
                      IConsoleService consoleService) : IMetadata
{
    private readonly Settings _settings = settings.Value;

    public void GenerateMedatadataFile(List<List<int>> seriesData)
    {
        consoleService.WriteHeading("Metadata");

        if (_settings.GenerateMetadataFile)
        {
            consoleService.Write("Generating metadata... ");

            string filePath = fileService.GenerateMetadataFilePath();

            if (fileService.FileExists(filePath))
            {
                consoleService.WriteLine("already exists\n");

                return;
            }

            StringBuilder content = new();

            content.Append(GenerateNumberSeriesMetadata(seriesData));
            content.Append(GenerateTop10LongestSeriesMetadata(seriesData));
            content.Append(GenerateFullSeriesData(seriesData));

            fileService.WriteMetadataToFile(content.ToString(), filePath);

            consoleService.WriteDone();
        }
        else
        {
            consoleService.WriteLine("Metadata generation disabled\n");
        }
    }

    /// <summary>
    /// Generate a list of the top 10 longest series resulting from running the algorithm on the generated or supplied numbers
    /// </summary>
    /// <param name="series"></param>
    /// <returns></returns>
    private static List<(int FirstNumber, int Count)> GenerateTop10LongestSeries(List<List<int>> series)
    {
        return series.Where(list => list.Count != 0).Select(list => (list.First(), list.Count))
                                                    .OrderByDescending(item => item.Count)
                                                    .Take(10)
                                                    .ToList();
    }

    /// <summary>
    /// Generate the human-readable number series metadata to store in the file
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Generate the human-readable top 10 longest series metadata to store in the file
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
    private static string GenerateTop10LongestSeriesMetadata(List<List<int>> seriesData)
    {
        StringBuilder content = new("\n\nTop 10 longest series:\n");

        foreach ((int FirstNumber, int Count) in GenerateTop10LongestSeries(seriesData))
        {
            content.Append($"{FirstNumber}: {Count} in series\n");
        }

        return content.ToString();
    }

    /// <summary>
    /// Generate the human-readable full lists of all number series produced by running the algorithm on the generated or supplied numbers
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
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