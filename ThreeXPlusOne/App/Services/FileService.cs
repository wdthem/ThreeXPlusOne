﻿using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public class FileService(IOptions<AppSettings> appSettings,
                         IConsoleService consoleService) : IFileService
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly string _prefix = Assembly.GetExecutingAssembly().GetName().Name!;
    private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

    /// <summary>
    /// Generate a full file path in which to store output files
    /// </summary>
    /// <param name="uniqueId"></param>
    /// <param name="path"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private string GenerateFullFilePath(string uniqueId, string? path, string fileName)
    {
        if (!string.IsNullOrWhiteSpace(path))
        {
            string? directory = Path.GetDirectoryName(path);

            if (directory == null || !Directory.Exists(directory))
            {
                throw new Exception($"Invalid {nameof(_appSettings.OutputPath)}. Check '{_appSettings.SettingsFileName}'");
            }
        }

        string newDirectoryName = $"{_prefix}-{uniqueId}";

        Directory.CreateDirectory(Path.Combine(path ?? "", newDirectoryName));

        return Path.Combine(path ?? "", newDirectoryName, fileName);
    }

    /// <summary>
    /// Get a filename-friendly timestamp to append to filenames
    /// </summary>
    /// <returns></returns>
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

        string jsonString = JsonSerializer.Serialize(_appSettings, _serializerOptions);

        File.WriteAllText(_appSettings.SettingsFileFullPath, jsonString);
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
            consoleService.WriteError(ex.Message);
        }
    }

    public string GenerateDirectedGraphFilePath()
    {
        string fileName = $"{_prefix}-{_appSettings.DirectedGraphAestheticSettings.SanitizedGraphDimensions}D-DirectedGraph-{GetFilenameTimestamp()}.png";

        return GenerateFullFilePath(_appSettings.UniqueExecutionId, _appSettings.OutputPath, fileName);
    }

    public string GenerateHistogramFilePath()
    {
        string fileName = $"{_prefix}-Histogram.png";

        return GenerateFullFilePath(_appSettings.UniqueExecutionId, _appSettings.OutputPath, fileName);
    }

    public string GenerateMetadataFilePath()
    {
        string fileName = $"{_prefix}-Metadata.txt";

        return GenerateFullFilePath(_appSettings.UniqueExecutionId, _appSettings.OutputPath, fileName);
    }
}