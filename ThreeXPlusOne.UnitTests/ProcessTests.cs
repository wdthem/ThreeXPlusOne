using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.App;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class ProcessTests
{
    private IOptions<AppSettings> _appSettings = new OptionsWrapper<AppSettings>
    (
        new AppSettings { }
    );

    private readonly Mock<IAlgorithmService> _algorithmMock;
    private readonly Mock<IDirectedGraph> _directedGraph;
    private readonly IEnumerable<IDirectedGraph> _directedGraphs;
    private readonly Mock<IHistogramService> _histogramMock;
    private readonly Mock<IMetadataService> _metadataMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IConsoleService> _consoleServiceMock;

    public ProcessTests()
    {
        _algorithmMock = new Mock<IAlgorithmService>();
        _directedGraph = new Mock<IDirectedGraph>();
        _directedGraph.Setup(graph => graph.Dimensions).Returns(2);
        _directedGraphs = [_directedGraph.Object];
        _histogramMock = new Mock<IHistogramService>();
        _metadataMock = new Mock<IMetadataService>();
        _fileServiceMock = new Mock<IFileService>();
        _consoleServiceMock = new Mock<IConsoleService>();
    }

    private void ResetSettings()
    {
        _appSettings = new OptionsWrapper<AppSettings>
        (
            new AppSettings { }
        );

        _appSettings.Value.DirectedGraphAestheticSettings.GraphDimensions = 2;
        _appSettings.Value.AlgorithmSettings.RandomNumberTotal = 10;
        _appSettings.Value.AlgorithmSettings.RandomNumberMax = 100;
    }

    [Fact]
    public void Run_Success00()
    {
        // Arrange
        ResetSettings();

        _algorithmMock.Setup(algorithm => algorithm.Run(It.IsAny<List<int>>())).Returns([[32, 16, 8, 4, 2, 1]]);
        _consoleServiceMock.Setup(consoleService => consoleService.ReadYKeyToProceed("Generate 2D visualization?")).Returns(true);

        var process = new Process(_appSettings,
                                  _algorithmMock.Object,
                                  _directedGraphs,
                                  _histogramMock.Object,
                                  _metadataMock.Object,
                                  _fileServiceMock.Object,
                                  _consoleServiceMock.Object);

        // Act
        process.Run([]);

        // Assert
        _algorithmMock.Verify(algorithm => algorithm.Run(It.IsAny<List<int>>()), Times.Once);
        _directedGraph.Verify(graph => graph.AddSeries(It.IsAny<List<List<int>>>()), Times.AtLeastOnce);
        _directedGraph.Verify(graph => graph.PositionNodes(), Times.Once);
        _directedGraph.Verify(graph => graph.SetNodeAesthetics(), Times.Once);
        _directedGraph.Verify(graph => graph.SetCanvasDimensions(), Times.Once);
        _directedGraph.Verify(graph => graph.Draw(), Times.Once);
        _histogramMock.Verify(histogram => histogram.GenerateHistogram(It.IsAny<List<List<int>>>()), Times.Once);
        _metadataMock.Verify(metadata => metadata.GenerateMedatadataFile(It.IsAny<List<List<int>>>()), Times.Once);
        _consoleServiceMock.Verify(helper => helper.ReadYKeyToProceed(It.IsAny<string>()), Times.Exactly(2));
        _fileServiceMock.Verify(helper => helper.WriteSettingsToFile(It.IsAny<bool>()), Times.Once);
        _consoleServiceMock.Verify(helper => helper.WriteSettingsSavedMessage(It.IsAny<bool>()), Times.Once);
    }

    /// <summary>
    /// User-provided input values result in no data to process
    /// </summary>
    [Fact]
    public void Run_Failure00()
    {
        // Arrange
        ResetSettings();

        _appSettings.Value.AlgorithmSettings.NumbersToUse = "5,6,7";
        _appSettings.Value.AlgorithmSettings.NumbersToExclude = "5,6,7";

        var process = new Process(_appSettings,
                                  _algorithmMock.Object,
                                  _directedGraphs,
                                  _histogramMock.Object,
                                  _metadataMock.Object,
                                  _fileServiceMock.Object,
                                  _consoleServiceMock.Object);

        // Act + Assert
        process.Invoking(process => process.Run([])).Should().Throw<Exception>();
    }
}