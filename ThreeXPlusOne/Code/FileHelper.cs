using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class FileHelper
{
    private static string GenerateFullFilePath(string? path, string fileName)
    {
        string fullPath;

        fullPath = Path.Combine(path ?? "", fileName);

        if (fullPath == fileName)
        {
            return fullPath;
        }

        var directory = Path.GetDirectoryName(fullPath);

        if (!Directory.Exists(directory))
        {
            return "";
        }

        return fullPath;
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

        var fileName = $"ThreeXPlusOne-{graphRotation}-{settings.FileNameUniqueId}.png";

        return GenerateFullFilePath(settings.OutputPath, fileName);
    }

    public static string GenerateHistogramFilePath(Settings settings)
    {
        var fileName = $"ThreeXPlusOne-Histogram-{settings.FileNameUniqueId}.png";

        return GenerateFullFilePath(settings.OutputPath, fileName);
    }

    public static string GenerateMetadataFilePath(Settings settings)
    {
        var fileName = $"ThreeXPlusOne-Metadata-{settings.FileNameUniqueId}.txt";

        return GenerateFullFilePath(settings.OutputPath, fileName);
    }
}