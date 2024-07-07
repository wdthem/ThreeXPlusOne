namespace ThreeXPlusOne.App.Interfaces.Services;

public interface IHistogramService : IScopedService
{
    /// <summary>
    /// Generate a histogram based on the given run of the process.
    /// The histogram shows the distribution of all generated numbers based on their starting digit (1-9) illustrating that the generated numbers
    /// follow Benford's law.
    /// </summary>
    /// <param name="seriesData"></param>
    void GenerateHistogram(List<List<int>> seriesData);
}