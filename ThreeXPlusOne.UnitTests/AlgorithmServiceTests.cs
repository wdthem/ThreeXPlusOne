using FluentAssertions;
using Moq;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Services;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class AlgorithmServiceTests
{
    private readonly Mock<IConsoleService> _consoleServiceMock;
    private readonly AlgorithmService _algorithmService;

    public AlgorithmServiceTests()
    {
        _consoleServiceMock = new Mock<IConsoleService>();
        _algorithmService = new AlgorithmService(_consoleServiceMock.Object);
    }

    /// <summary>
    /// For non-root numbers (not 4, 2, 1).
    /// </summary>
    [Fact]
    public void AlgorithmReturnsSeriesWithExpectedEnd_00()
    {
        // Arrange
        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedEndingSeries = [16, 8, 4, 2, 1];

        // Act
        List<CollatzResult> results = _algorithmService.Run(startingNumbers);

        // Assert
        foreach (CollatzResult result in results)
        {
            bool seriesEndMatch = Enumerable.SequenceEqual(result.Values.Skip(result.Values.Count - expectedEndingSeries.Count), expectedEndingSeries);

            result.Values.Count.Should().BeGreaterThanOrEqualTo(expectedEndingSeries.Count);
            seriesEndMatch.Should().BeTrue();
        }
    }

    /// <summary>
    /// For root numbers (4, 2, 1).
    /// </summary>
    [Fact]
    public void AlgorithmReturnsSeriesWithExpectedEnd_01()
    {
        // Arrange
        List<int> startingNumbers = [4, 2, 1];
        List<int> expectedEndingSeriesNumberCounts = [3, 2, 1];

        // Act
        List<CollatzResult> results = _algorithmService.Run(startingNumbers);

        // Assert
        foreach (CollatzResult result in results)
        {
            bool hasExpectedCount = expectedEndingSeriesNumberCounts.Contains(result.Values.Count);
            bool hasExpectedNumbers = result.Values.All(startingNumbers.Contains);

            hasExpectedCount.Should().BeTrue();
            hasExpectedNumbers.Should().BeTrue();
        }
    }

    /// <summary>
    /// For non-root numbers (not 4, 2, 1).
    /// </summary>
    [Fact]
    public void AlgorithmReturnsExpectedStoppingTimes_00()
    {
        // Arrange
        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedStoppingTimes = [3, 11, 1, 3, 6];
        List<int> expectedTotalStoppingTimes = [5, 16, 9, 26, 31];

        // Act
        List<CollatzResult> results = _algorithmService.Run(startingNumbers);

        // Assert
        int lcv = 0;

        foreach (CollatzResult result in results)
        {
            result.StoppingTime.Should().Be(expectedStoppingTimes[lcv]);
            result.TotalStoppingTime.Should().Be(expectedTotalStoppingTimes[lcv]);

            lcv++;
        }
    }

    /// <summary>
    /// For negative numbers.
    /// </summary>
    [Fact]
    public void AlgorithmReturnsEmptyList()
    {
        // Arrange
        List<int> startingNumbers = [-3, -29, -824];

        // Act
        List<CollatzResult> results = _algorithmService.Run(startingNumbers);

        // Assert
        foreach (CollatzResult result in results)
        {
            result.Values.Count.Should().Be(0);
        }
    }

    /// <summary>
    /// For empty input list.
    /// </summary>
    [Fact]
    public void AlgorithmThrowsExceptionForEmptyInput()
    {
        // Arrange
        List<int> startingNumbers = [];

        // Act + Assert
        _algorithmService.Invoking(algorithm => algorithm.Run(startingNumbers)).Should().Throw<Exception>();
    }
}