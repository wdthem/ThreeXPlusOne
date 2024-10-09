using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.App;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class ProcessTests
{
    private IOptions<AppSettings> _appSettings = new OptionsWrapper<AppSettings>
    (
        new AppSettings { }
    );

    private readonly Mock<IAlgorithmService> _algorithmServiceMock;
    private readonly Mock<IDirectedGraph> _directedGraph;
    private readonly IEnumerable<IDirectedGraph> _directedGraphs;
    private readonly Mock<IHistogramService> _histogramServiceMock;
    private readonly Mock<IMetadataService> _metadataServiceMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IConsoleService> _consoleServiceMock;

    public ProcessTests()
    {
        _algorithmServiceMock = new Mock<IAlgorithmService>();
        _directedGraph = new Mock<IDirectedGraph>();
        _directedGraph.Setup(graph => graph.GraphType).Returns(GraphType.Standard2D);
        _directedGraphs = [_directedGraph.Object];
        _histogramServiceMock = new Mock<IHistogramService>();
        _metadataServiceMock = new Mock<IMetadataService>();
        _fileServiceMock = new Mock<IFileService>();
        _consoleServiceMock = new Mock<IConsoleService>();
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

    [Fact]
    public async Task Run_Success00()
    {
        // Arrange
        ResetSettings();

        _algorithmServiceMock.Setup(algorithm => algorithm.Run(It.IsAny<List<int>>())).Returns([new CollatzResult() { Values = [32, 16, 8, 4, 2, 1] }]);
        _consoleServiceMock.Setup(consoleService => consoleService.ReadYKeyToProceed("Generate Standard2D visualization?")).Returns(true);

        var process = new Process(_appSettings,
                                  _algorithmServiceMock.Object,
                                  _directedGraphs,
                                  _histogramServiceMock.Object,
                                  _metadataServiceMock.Object,
                                  _fileServiceMock.Object,
                                  _consoleServiceMock.Object);

        // Act
        await process.Run([]);

        // Assert
        _algorithmServiceMock.Verify(algorithm => algorithm.Run(It.IsAny<List<int>>()), Times.Once);
        _directedGraph.Verify(graph => graph.AddSeries(It.IsAny<List<List<int>>>()), Times.AtLeastOnce);
        _directedGraph.Verify(graph => graph.PositionNodes(), Times.Once);
        _directedGraph.Verify(graph => graph.SetNodeAesthetics(), Times.Once);
        _directedGraph.Verify(graph => graph.SetCanvasDimensions(), Times.Once);
        _directedGraph.Verify(graph => graph.Draw(), Times.Once);
        _histogramServiceMock.Verify(histogram => histogram.GenerateHistogram(It.IsAny<List<List<int>>>()), Times.Once);
        _metadataServiceMock.Verify(metadata => metadata.GenerateMedatadataFile(It.IsAny<List<List<int>>>()), Times.Once);
        _consoleServiceMock.Verify(helper => helper.ReadYKeyToProceed(It.IsAny<string>()), Times.Exactly(2));
        _fileServiceMock.Verify(helper => helper.WriteSettingsToFile(It.IsAny<bool>()), Times.Once);
        _consoleServiceMock.Verify(helper => helper.WriteSettingsSavedMessage(It.IsAny<bool>()), Times.Once);
    }

    /// <summary>
    /// User-provided input values result in no data to process.
    /// </summary>
    [Fact]
    public async Task Run_Failure00()
    {
        // Arrange
        ResetSettings();

        _appSettings.Value.AlgorithmSettings.NumbersToUse = "5,6,7";
        _appSettings.Value.AlgorithmSettings.NumbersToExclude = "5,6,7";

        var process = new Process(_appSettings,
                                  _algorithmServiceMock.Object,
                                  _directedGraphs,
                                  _histogramServiceMock.Object,
                                  _metadataServiceMock.Object,
                                  _fileServiceMock.Object,
                                  _consoleServiceMock.Object);

        // Act + Assert
        await process.Invoking(process => process.Run([])).Should().ThrowAsync<Exception>();
    }
}