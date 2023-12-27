using System.Text.Json;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code.Helpers;

public class FileHelper(IOptions<Settings> settings,
                        IConsoleHelper consoleHelper) : IFileHelper
{
    private readonly Settings _settings = settings.Value;
    private readonly string _prefix = "ThreeXPlusOne";
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    private string GenerateFullFilePath(string uniqueId, string? path, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            var directory = Path.GetDirectoryName(path);

            if (!Directory.Exists(directory))
            {
                throw new Exception($"Invalid {nameof(_settings.OutputPath)}. Check '{_settings.SettingsFileName}'");
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

    public void WriteSettingsToFile(bool userConfirmedSave)
    {
        if (!userConfirmedSave)
        {
            return;
        }

        string jsonString = JsonSerializer.Serialize(_settings, _serializerOptions);

        File.WriteAllText(_settings.SettingsFileName, jsonString);
    }

    public bool FileExists(string filePath)
    {
        return File.Exists(filePath);
    }

    public void WriteMetadataToFile(string content, string filePath)
    {
        try
        {
            using StreamWriter writer = new(filePath, false);

            writer.WriteLine(content);
        }
        catch (Exception ex)
        {
            consoleHelper.WriteError(ex.Message);
        }
    }

    public string GenerateDirectedGraphFilePath()
    {
        var fileName = $"{_prefix}-{_settings.SanitizedGraphDimensions}D-DirectedGraph-{GetFilenameTimestamp()}.png";

        return GenerateFullFilePath(_settings.UniqueExecutionId, _settings.OutputPath, fileName);
    }

    public string GenerateHistogramFilePath()
    {
        var fileName = $"{_prefix}-Histogram.png";

        return GenerateFullFilePath(_settings.UniqueExecutionId, _settings.OutputPath, fileName);
    }

    public string GenerateMetadataFilePath()
    {
        var fileName = $"{_prefix}-Metadata.txt";

        return GenerateFullFilePath(_settings.UniqueExecutionId, _settings.OutputPath, fileName);
    }
}