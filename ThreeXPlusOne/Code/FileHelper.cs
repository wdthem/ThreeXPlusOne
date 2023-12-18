using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class FileHelper(IOptions<Settings> settings) : IFileHelper
{
    private readonly IOptions<Settings> _settings = settings;

    private string GenerateFullFilePath(string uniqueId, string? path, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                throw new Exception($"Invalid {nameof(_settings.Value.OutputPath)}. Check 'settings.json'");
            }
        }

        string newDirectoryName = $"ThreeXPlusOne-{uniqueId}";

        Directory.CreateDirectory(Path.Combine(path ?? "", newDirectoryName));

        return Path.Combine(path ?? "", newDirectoryName, fileName);
    }

    private static string GetFilenameTimestamp()
    {
        return DateTime.Now.ToString("yyyyMMdd-HHmmss");
    }

    public void WriteMetadataToFile(string content, string filePath)
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

    public string GenerateDirectedGraphFilePath()
    {
        var fileName = $"ThreeXPlusOne-{_settings.Value.ParsedGraphDimensions}D-DirectedGraph-{GetFilenameTimestamp()}.png";

        return GenerateFullFilePath(_settings.Value.UniqueExecutionId, _settings.Value.OutputPath, fileName);
    }

    public string GenerateHistogramFilePath()
    {
        var fileName = $"ThreeXPlusOne-Histogram-{GetFilenameTimestamp()}.png";

        return GenerateFullFilePath(_settings.Value.UniqueExecutionId, _settings.Value.OutputPath, fileName);
    }

    public string GenerateMetadataFilePath()
    {
        var fileName = $"ThreeXPlusOne-Metadata-{GetFilenameTimestamp()}.txt";

        return GenerateFullFilePath(_settings.Value.UniqueExecutionId, _settings.Value.OutputPath, fileName);
    }
}