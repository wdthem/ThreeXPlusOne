namespace ThreeXPlusOne.App.Interfaces.Services;

public interface IMetadataService : IScopedService
{
    /// <summary>
    /// Generate a file with various metadata about the specific run of process with the generated or supplied numbers.
    /// </summary>
    /// <param name="seriesData"></param>
    void GenerateMedatadataFile(List<List<int>> seriesData);
}