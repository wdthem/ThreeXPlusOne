namespace ThreeXPlusOne.App.Interfaces.Services;

public interface IMetadataService : IScopedService
{
    /// <summary>
    /// Generate the metadata and histogram based on the lists of series numbers.
    /// </summary>
    /// <param name="seriesData"></param>
    Task GenerateMetadata(List<List<int>> seriesData);
}