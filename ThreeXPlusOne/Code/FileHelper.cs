using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class FileHelper : IFileHelper
{
    private readonly IOptions<Settings> _settings;

    public FileHelper(IOptions<Settings> settings)
    {
        _settings = settings;
    }

    private string GenerateFullFilePath(string uniqueId, string? path, string fileName)
    {
        if (!string.IsNullOrEmpty(path))
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

    public string GenerateGraphFilePath()
	{
        var fileName = $"ThreeXPlusOne-{_settings.Value.ParsedGraphDimensions}D-DirectedGraph.png";

        return GenerateFullFilePath(_settings.Value.UniqueExecutionId, _settings.Value.OutputPath, fileName);
    }

    public string GenerateHistogramFilePath()
    {
        var fileName = $"ThreeXPlusOne-Histogram.png";

        return GenerateFullFilePath(_settings.Value.UniqueExecutionId, _settings.Value.OutputPath, fileName);
    }

    public string GenerateMetadataFilePath()
    {
        var fileName = $"ThreeXPlusOne-Metadata.txt";

        return GenerateFullFilePath(_settings.Value.UniqueExecutionId, _settings.Value.OutputPath, fileName);
    }
}