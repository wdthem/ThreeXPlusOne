using FluentAssertions;
using Moq;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Code.Interfaces.Helpers;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class AlgorithmTests
{
    private readonly Mock<IConsoleHelper> _consoleHelperMock;
    private readonly Algorithm _algorithm;

    public AlgorithmTests()
    {
        _consoleHelperMock = new Mock<IConsoleHelper>();
        _algorithm = new Algorithm(_consoleHelperMock.Object);
    }

    /// <summary>
    /// For non-root numbers (not 4, 2, 1)
    /// </summary>
    [Fact]
    public void AlgorithmReturnsSeriesWithExpectedEnd_00()
    {
        // Arrange
        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedEndingSeries = [16, 8, 4, 2, 1];

        // Act
        List<List<int>> results = _algorithm.Run(startingNumbers);

        // Assert
        foreach (List<int> series in results)
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
    public void AlgorithmReturnsSeriesWithExpectedEnd_01()
    {
        // Arrange
        List<int> startingNumbers = [4, 2, 1];
        List<int> expectedEndingSeriesNumberCounts = [3, 2, 1];

        // Act
        List<List<int>> results = _algorithm.Run(startingNumbers);

        // Assert
        foreach (List<int> series in results)
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
    public void AlgorithmReturnsEmptyList()
    {
        // Arrange
        List<int> startingNumbers = [-3, -29, -824];

        // Act
        List<List<int>> results = _algorithm.Run(startingNumbers);

        // Assert
        foreach (List<int> series in results)
        {
            series.Count.Should().Be(0);
        }
    }

    /// <summary>
    /// For empty input list
    /// </summary>
    [Fact]
    public void AlgorithmThrowsExceptionForEmptyInput()
    {
        // Arrange
        List<int> startingNumbers = [];

        // Act + Assert
        _algorithm.Invoking(algorithm => algorithm.Run(startingNumbers)).Should().Throw<Exception>();
    }
}