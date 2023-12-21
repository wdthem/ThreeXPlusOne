using FluentAssertions;
using ThreeXPlusOne.Code;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class AlgorithmTests
{
    private readonly Algorithm _algorithm = new();

    /// <summary>
    /// For non-root numbers (not 4, 2, 1)
    /// </summary>
    [Fact]
    public void AlgorithmReturnsSeriesWithExpectedEnd_Success00()
    {
        // Arrange
        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedEndingSeries = [16, 8, 4, 2, 1];

        // Act
        List<List<int>> results = _algorithm.Run(startingNumbers);

        // Assert
        foreach (var series in results)
        {
            bool seriesEndMatch = Enumerable.SequenceEqual(series.Skip(series.Count - expectedEndingSeries.Count), expectedEndingSeries);

            series.Count.Should().BeGreaterThanOrEqualTo(expectedEndingSeries.Count);
            seriesEndMatch.Should().BeTrue();
        }
    }

    /// <summary>
    /// For root numbers (4, 2, 1)
    /// </summary>
    [Fact]
    public void AlgorithmReturnsSeriesWithExpectedEnd_Success01()
    {
        // Arrange
        List<int> startingNumbers = [4, 2, 1];
        List<int> expectedEndingSeriesNumberCounts = [3, 2, 1];

        // Act
        List<List<int>> results = _algorithm.Run(startingNumbers);

        // Assert
        foreach (var series in results)
        {
            bool hasExpectedCount = expectedEndingSeriesNumberCounts.Contains(series.Count);
            bool hasExpectedNumbers = series.All(item => startingNumbers.Contains(item));

            hasExpectedCount.Should().BeTrue();
            hasExpectedNumbers.Should().BeTrue();
        }
    }

    /// <summary>
    /// For negative numbers
    /// </summary>
    [Fact]
    public void AlgorithmReturnsEmptyList_Success00()
    {
        // Arrange
        List<int> startingNumbers = [-3, -29, -824];

        // Act
        List<List<int>> results = _algorithm.Run(startingNumbers);

        // Assert
        foreach (var series in results)
        {
            series.Count.Should().Be(0);
        }
    }
}