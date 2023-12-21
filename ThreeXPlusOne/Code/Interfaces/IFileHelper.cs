﻿namespace ThreeXPlusOne.Code.Interfaces;

public interface IFileHelper
{
    /// <summary>
    /// Save the Settings object values back to settings.json
    /// </summary>
    void WriteSettingsToFile();

    /// <summary>
    /// Save the metadata generated by the execution of the process to a file
    /// </summary>
    /// <param name="content"></param>
    /// <param name="filePath"></param>
    void WriteMetadataToFile(string content, string filePath);

    /// <summary>
    /// Create a directory and return the full path to the graph file
    /// </summary>
    /// <returns></returns>
    string GenerateDirectedGraphFilePath();

    /// <summary>
    /// Create a directory (if required) and return the full path to the histogram file
    /// </summary>
    /// <returns></returns>
    string GenerateHistogramFilePath();

    /// <summary>
    /// Create a directory (if required) and return the full path to the metadata file
    /// </summary>
    /// <returns></returns>
    string GenerateMetadataFilePath();
}