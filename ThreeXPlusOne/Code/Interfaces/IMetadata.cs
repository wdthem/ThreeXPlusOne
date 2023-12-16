namespace ThreeXPlusOne.Code.Interfaces;

public interface IMetadata
{
    /// <summary>
    /// Generate a file with various metadata about the given run of the process
    /// </summary>
    /// <param name="seriesData"></param>
    void GenerateMedatadataFile(List<List<int>> seriesData);
}