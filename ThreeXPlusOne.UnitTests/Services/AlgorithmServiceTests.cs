using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Services;
using ThreeXPlusOne.App.Services.Interfaces;
using Xunit;

namespace ThreeXPlusOne.UnitTests.Services;

public class AlgorithmServiceTests
{
    private IOptions<AppSettings> _appSettings = new OptionsWrapper<AppSettings>
    (
        new AppSettings { }
    );

    private readonly Mock<IAlgorithmPresenter> _algorithmPresenterMock;
    private readonly Mock<IMetadataService> _metadataServiceMock;

    public AlgorithmServiceTests()
    {
        _algorithmPresenterMock = new Mock<IAlgorithmPresenter>();
        _metadataServiceMock = new Mock<IMetadataService>();
    }

    private void ResetSettings()
    {
        _appSettings = new OptionsWrapper<AppSettings>
        (
            new AppSettings { }
        );

        _appSettings.Value.DirectedGraphAestheticSettings.GraphType = "Standard2D";
        _appSettings.Value.AlgorithmSettings.RandomNumberTotal = 10;
        _appSettings.Value.AlgorithmSettings.RandomNumberMax = 100;
    }

    /// <summary>
    /// For non-root numbers (not 4, 2, 1).
    /// </summary>
    [Fact]
    public async Task StandardAlgorithmReturnsSeriesWithExpectedEnd_00()
    {
        // Arrange
        ResetSettings();

        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedEndingSeries = [16, 8, 4, 2, 1];

        _appSettings.Value.AlgorithmSettings.NumbersToUse = string.Join(", ", startingNumbers);

        var algorithmService = new AlgorithmService(_appSettings,
                                                    _metadataServiceMock.Object,
                                                    _algorithmPresenterMock.Object);

        // Act
        List<CollatzResult> results = await algorithmService.Run();

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
    public async Task StandardAlgorithmReturnsSeriesWithExpectedEnd_01()
    {
        // Arrange
        ResetSettings();

        List<int> startingNumbers = [4, 2, 1];
        List<int> expectedEndingSeriesNumberCounts = [3, 2, 1];

        _appSettings.Value.AlgorithmSettings.NumbersToUse = string.Join(", ", startingNumbers);

        var algorithmService = new AlgorithmService(_appSettings,
                                                    _metadataServiceMock.Object,
                                                    _algorithmPresenterMock.Object);

        // Act
        List<CollatzResult> results = await algorithmService.Run();

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
    public async Task StandardAlgorithmReturnsExpectedStoppingTimes_00()
    {
        // Arrange
        ResetSettings();

        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedStoppingTimes = [3, 11, 1, 3, 6];
        List<int> expectedTotalStoppingTimes = [5, 16, 9, 26, 31];

        _appSettings.Value.AlgorithmSettings.NumbersToUse = string.Join(", ", startingNumbers);

        var algorithmService = new AlgorithmService(_appSettings,
                                                    _metadataServiceMock.Object,
                                                    _algorithmPresenterMock.Object);

        // Act
        List<CollatzResult> results = await algorithmService.Run();

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
    /// For non-root numbers (not 4, 2, 1).
    /// </summary>
    [Fact]
    public async Task ShortcutAlgorithmReturnsSeriesWithExpectedEnd()
    {
        // Arrange
        ResetSettings();

        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedEndingSeries = [5, 16, 1];

        _appSettings.Value.AlgorithmSettings.UseShortcutAlgorithm = true;
        _appSettings.Value.AlgorithmSettings.NumbersToUse = string.Join(", ", startingNumbers);

        var algorithmService = new AlgorithmService(_appSettings,
                                                    _metadataServiceMock.Object,
                                                    _algorithmPresenterMock.Object);

        // Act
        List<CollatzResult> results = await algorithmService.Run();

        // Assert
        foreach (CollatzResult result in results)
        {
            bool seriesEndMatch = Enumerable.SequenceEqual(result.Values.Skip(result.Values.Count - expectedEndingSeries.Count), expectedEndingSeries);

            result.Values.Count.Should().BeGreaterThanOrEqualTo(expectedEndingSeries.Count);
            seriesEndMatch.Should().BeTrue();
        }
    }

    /// <summary>
    /// For non-root numbers (not 4, 2, 1).
    /// </summary>
    [Fact]
    public async Task ShortcutAlgorithmReturnsExpectedStoppingTimes_00()
    {
        // Arrange
        ResetSettings();

        List<int> startingNumbers = [5, 7, 12, 33, 179];
        List<int> expectedStoppingTimes = [2, 8, 1, 2, 4];
        List<int> expectedTotalStoppingTimes = [2, 10, 5, 16, 18];

        _appSettings.Value.AlgorithmSettings.UseShortcutAlgorithm = true;
        _appSettings.Value.AlgorithmSettings.NumbersToUse = string.Join(", ", startingNumbers);

        var algorithmService = new AlgorithmService(_appSettings,
                                                    _metadataServiceMock.Object,
                                                    _algorithmPresenterMock.Object);

        // Act
        List<CollatzResult> results = await algorithmService.Run();

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
    /// Test where passing in only negative numbers means the appsettings.AlgorithmSettings.FromRandomNumbers is true because
    /// it ignores the negative numbers, then has no numbers to use, and so generates random numbers.
    /// </summary>
    [Fact]
    public async Task PassingOnlyNegativeNumbersCausesFromRandomNumbersToBeTrue_00()
    {
        // Arrange
        ResetSettings();

        List<int> startingNumbers = [-5, -7, -12, -33, -179];

        _appSettings.Value.AlgorithmSettings.FromRandomNumbers = true;
        _appSettings.Value.AlgorithmSettings.NumbersToUse = string.Join(", ", startingNumbers);

        var algorithmService = new AlgorithmService(_appSettings,
                                                    _metadataServiceMock.Object,
                                                    _algorithmPresenterMock.Object);

        // Act
        await algorithmService.Run();

        // Assert
        _appSettings.Value.AlgorithmSettings.FromRandomNumbers.Should().BeTrue();
    }

    /// <summary>
    /// Test where negative numbers are removed from the series list because they are not valid inputs for the algorithm.
    /// </summary>
    [Fact]
    public async Task NegativeNumbersAreRemovedFromSeriesList_00()
    {
        // Arrange
        ResetSettings();

        List<int> startingNumbers = [5, 7, -12];

        _appSettings.Value.AlgorithmSettings.FromRandomNumbers = true;
        _appSettings.Value.AlgorithmSettings.NumbersToUse = string.Join(", ", startingNumbers);

        var algorithmService = new AlgorithmService(_appSettings,
                                                    _metadataServiceMock.Object,
                                                    _algorithmPresenterMock.Object);

        // Act
        List<CollatzResult> results = await algorithmService.Run();

        // Assert
        results.Count.Should().Be(2);
    }
}
