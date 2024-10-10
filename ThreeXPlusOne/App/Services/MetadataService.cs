using System.Text;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services;

public class MetadataService(IFileService fileService,
                             IHistogramService histogramService,
                             IConsoleService consoleService) : IMetadataService
{
    /// <summary>
    /// Generate the metadata and histogram based on the lists of series numbers.
    /// </summary>
    /// <param name="collatzResults"></param>
    /// <returns></returns>
    public async Task GenerateMetadata(List<CollatzResult> collatzResults)
    {
        consoleService.WriteHeading("Metadata");

        await GenerateSeriesMetadataFile(collatzResults);
        await histogramService.GenerateHistogram(collatzResults);
    }

    /// <summary>
    /// Generate the metadata based on the lists of series numbers.
    /// </summary>
    /// <param name="collatzResults"></param>
    private async Task GenerateSeriesMetadataFile(List<CollatzResult> collatzResults)
    {
        consoleService.Write("Generating number series metadata... ");

        string filePath = fileService.GenerateMetadataFilePath();

        if (fileService.FileExists(filePath))
        {
            consoleService.WriteLine("already exists\n");

            return;
        }

        StringBuilder content = new();

        content.Append(GenerateNumberSeriesMetadata(collatzResults));
        content.Append(GenerateTop10LongestSeriesMetadata(collatzResults));
        content.Append(GenerateFullSeriesData(collatzResults));

        await fileService.WriteMetadataToFile(content.ToString(), filePath);

        consoleService.WriteDone();
    }

    /// <summary>
    /// Generate a list of the top 10 longest series resulting from running the algorithm on the generated or supplied numbers.
    /// </summary>
    /// <param name="series"></param>
    /// <returns></returns>
    private static List<(int FirstNumber, int Count)> GenerateTop10LongestSeries(List<CollatzResult> collatzResults)
    {
        return collatzResults.Where(result => result.Values.Count != 0)
                             .Select(result => (result.Values[0], result.Values.Count))
                             .OrderByDescending(item => item.Count)
                             .Take(10)
                             .ToList();
    }

    /// <summary>
    /// Generate the human-readable number series metadata to store in the file.
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
    private static string GenerateNumberSeriesMetadata(List<CollatzResult> collatzResults)
    {
        StringBuilder content = new("\nSeries run for the following numbers: \n");

        int lcv = 1;

        foreach (CollatzResult collatzResult in collatzResults)
        {
            content.Append($"{collatzResult.Values[0]}, ");

            lcv++;
        }

        return content.ToString();
    }

    /// <summary>
    /// Generate the human-readable top 10 longest series metadata to store in the file.
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
    private static string GenerateTop10LongestSeriesMetadata(List<CollatzResult> collatzResults)
    {
        StringBuilder content = new("\n\nTop 10 longest series:\n");

        foreach ((int FirstNumber, int Count) in GenerateTop10LongestSeries(collatzResults))
        {
            content.Append($"{FirstNumber}: {Count} in series\n");
        }

        return content.ToString();
    }

    /// <summary>
    /// Generate the human-readable full lists of all number series produced by running the algorithm on the generated or supplied numbers.
    /// </summary>
    /// <param name="seriesData"></param>
    /// <returns></returns>
    private static string GenerateFullSeriesData(List<CollatzResult> collatzResults)
    {
        StringBuilder content = new("\nFull series data:\n");

        foreach (CollatzResult collatzResult in collatzResults)
        {
            content.Append(string.Join(", ", collatzResult.Values));
            content.Append("\n\n");
        }

        return content.ToString();
    }
}