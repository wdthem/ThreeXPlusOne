using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class FileHelper
{
    private static string GenerateFullFilePath(string uniqueId, string? path, string fileName)
    {
        if (!string.IsNullOrEmpty(path))
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                return "";
            }
        }

        string newDirectoryName = $"ThreeXPlusOne-{uniqueId}";

        Directory.CreateDirectory(Path.Combine(path ?? "", newDirectoryName));

        return Path.Combine(path ?? "", uniqueId, fileName);
    }

    public static void WriteMetadataToFile(string content, string filePath)
    {
        try
        {
            using StreamWriter writer = new(filePath, true);

            writer.WriteLine(content);
        }
        catch (Exception ex)
        {
            ConsoleOutput.WriteError(ex.Message);
        }
    }

    public static string GenerateGraphFilePath(Settings settings)
	{
        var graphRotation = settings.NodeRotationAngle == 0 ? "NoRotation" : "Rotation";

        var fileName = $"ThreeXPlusOne-DirectedGraph-{graphRotation}.png";

        return GenerateFullFilePath(settings.UniqueExecutionId, settings.OutputPath, fileName);
    }

    public static string GenerateHistogramFilePath(Settings settings)
    {
        var fileName = $"ThreeXPlusOne-Histogram.png";

        return GenerateFullFilePath(settings.UniqueExecutionId, settings.OutputPath, fileName);
    }

    public static string GenerateMetadataFilePath(Settings settings)
    {
        var fileName = $"ThreeXPlusOne-Metadata.txt";

        return GenerateFullFilePath(settings.UniqueExecutionId, settings.OutputPath, fileName);
    }
}