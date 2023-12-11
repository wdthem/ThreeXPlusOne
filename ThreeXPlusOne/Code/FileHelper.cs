using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class FileHelper
{
	public static string GenerateGraphFilePath(Settings settings)
	{
        string fullPath;

        var graphRotation = settings.RotationAngle == 0 ? "NoRotation" : "Rotation";

        var outputFileName = $"ThreeXPlusOne-{graphRotation}-{settings.FileNameUniqueId}.png";

        fullPath = Path.Combine(settings.OutputPath ?? "", outputFileName);

        if (fullPath == outputFileName)
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

    public static string GenerateHistogramFilePath(Settings settings)
    {
        string fullPath;

        var outputFileName = $"ThreeXPlusOne-Histogram-{settings.FileNameUniqueId}.png";

        fullPath = Path.Combine(settings.OutputPath ?? "", outputFileName);

        if (fullPath == outputFileName)
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
}