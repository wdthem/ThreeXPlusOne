using System.Text.Json;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class FileHelper(IOptions<Settings> settings,
                        IConsoleHelper consoleHelper) : IFileHelper
{
    private readonly string _prefix = "ThreeXPlusOne";
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private string GenerateFullFilePath(string uniqueId, string? path, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                throw new Exception($"Invalid {nameof(settings.Value.OutputPath)}. Check '{settings.Value.SettingsFileName}'");
            }
        }

        string newDirectoryName = $"{_prefix}-{uniqueId}";

        Directory.CreateDirectory(Path.Combine(path ?? "", newDirectoryName));

        return Path.Combine(path ?? "", newDirectoryName, fileName);
    }

    private static string GetFilenameTimestamp()
    {
        return DateTime.Now.ToString("yyyyMMdd-HHmmss");
    }

    public void WriteSettingsToFile()
    {
        string jsonString = JsonSerializer.Serialize(settings.Value, _serializerOptions);

        File.WriteAllText(settings.Value.SettingsFileName, jsonString);
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
            consoleHelper.WriteError(ex.Message);
        }
    }

    public string GenerateDirectedGraphFilePath()
    {
        var fileName = $"{_prefix}-{settings.Value.SanitizedGraphDimensions}D-DirectedGraph-{GetFilenameTimestamp()}.png";

        return GenerateFullFilePath(settings.Value.UniqueExecutionId, settings.Value.OutputPath, fileName);
    }

    public string GenerateHistogramFilePath()
    {
        var fileName = $"{_prefix}-Histogram-{GetFilenameTimestamp()}.png";

        return GenerateFullFilePath(settings.Value.UniqueExecutionId, settings.Value.OutputPath, fileName);
    }

    public string GenerateMetadataFilePath()
    {
        var fileName = $"{_prefix}-Metadata-{GetFilenameTimestamp()}.txt";

        return GenerateFullFilePath(settings.Value.UniqueExecutionId, settings.Value.OutputPath, fileName);
    }
}