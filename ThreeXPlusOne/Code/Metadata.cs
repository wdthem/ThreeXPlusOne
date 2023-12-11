using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Metadata
{
    public static void GenerateMedatadataFile(Settings settings, List<List<int>> seriesData)
    {
        Console.WriteLine();
        ConsoleOutput.WriteHeading("Metadata");

        if (settings.GenerateMetadataFile)
        {
            Console.Write("Generating metadata... ");

            GenerateNumberSeriesMetadata(settings, seriesData);
            GenerateTop10LongestSeriesMetadata(settings, seriesData);

            ConsoleOutput.WriteDone();
        }
        else
        {
            Console.WriteLine("Metadata generation disabled");
        }
    }

    private static List<(int FirstNumber, int Count)> GenerateTop10Series(List<List<int>> series)
    {
        return series.Where(list => list.Any()).Select(list => (list.First(), list.Count))
                                               .OrderByDescending(item => item.Count)
                                               .Take(10)
                                               .ToList();
    }

    public static void GenerateNumberSeriesMetadata(Settings settings, List<List<int>> seriesData)
	{
        var filePath = FileHelper.GenerateMetadataFilePath(settings);

        if (string.IsNullOrEmpty(filePath))
        {
            ConsoleOutput.WriteError("Invalid metadata file path. Check 'settings.json'");

            return;
        }

        string content = "\nSeries run for the following numbers: \n";

        var lcv = 1;

        foreach (List<int> values in seriesData)
        {
            if (lcv % 20 == 0)
            {
                content += "\n";
            }

            content += ($"{values[0]}, ");

            lcv++;
        }

        FileHelper.WriteMetadataToFile(content, filePath);
    }

    public static void GenerateTop10LongestSeriesMetadata(Settings settings, List<List<int>> seriesData)
    {
        var filePath = FileHelper.GenerateMetadataFilePath(settings);

        if (string.IsNullOrEmpty(filePath))
        {
            ConsoleOutput.WriteError("Invalid metadata file path. Check 'settings.json'");

            return;
        }

        string content = "\nTop 10 longest series:\n";

        foreach ((int FirstNumber, int Count) in GenerateTop10Series(seriesData))
        {
            content += $"{FirstNumber}: {Count} in series\n";
        }

        FileHelper.WriteMetadataToFile(content, filePath);
    }
}