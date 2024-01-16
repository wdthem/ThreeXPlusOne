using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class ProcessTests
{
    private readonly IOptions<Settings> _settings = new OptionsWrapper<Settings>
    (
        new Settings { GraphDimensions = 2, NumberOfSeries = 10, MaxStartingNumber = 100 }
    );

    private readonly Mock<IAlgorithm> _algorithmMock;
    private readonly Mock<IDirectedGraph> _directedGraph;
    private readonly IEnumerable<IDirectedGraph> _directedGraphs;
    private readonly Mock<IHistogram> _histogramMock;
    private readonly Mock<IMetadata> _metadataMock;
    private readonly Mock<IFileHelper> _fileHelperMock;
    private readonly Mock<IConsoleHelper> _consoleHelperMock;

    public ProcessTests()
    {
        _algorithmMock = new Mock<IAlgorithm>();
        _directedGraph = new Mock<IDirectedGraph>();
        _directedGraph.Setup(graph => graph.Dimensions).Returns(2);
        _directedGraphs = new List<IDirectedGraph> { _directedGraph.Object };
        _histogramMock = new Mock<IHistogram>();
        _metadataMock = new Mock<IMetadata>();
        _fileHelperMock = new Mock<IFileHelper>();
        _consoleHelperMock = new Mock<IConsoleHelper>();
    }

    [Fact]
    public void Run_Success00()
    {
        // Arrange
        _algorithmMock.Setup(algorithm => algorithm.Run(It.IsAny<List<int>>())).Returns([[32, 16, 8, 4, 2, 1]]);
        _consoleHelperMock.Setup(consoleHelper => consoleHelper.ReadYKeyToProceed("Generate 2D visualization?")).Returns(true);

        var process = new Process(_settings,
                                  _algorithmMock.Object,
                                  _directedGraphs,
                                  _histogramMock.Object,
                                  _metadataMock.Object,
                                  _fileHelperMock.Object,
                                  _consoleHelperMock.Object);

        // Act
        process.Run();

        // Assert
        _algorithmMock.Verify(algorithm => algorithm.Run(It.IsAny<List<int>>()), Times.Once);
        _directedGraph.Verify(graph => graph.AddSeries(It.IsAny<List<List<int>>>()), Times.AtLeastOnce);
        _directedGraph.Verify(graph => graph.PositionNodes(), Times.Once);
        _directedGraph.Verify(graph => graph.SetNodeShapes(), Times.Once);
        _directedGraph.Verify(graph => graph.SetCanvasDimensions(), Times.Once);
        _directedGraph.Verify(graph => graph.Draw(), Times.Once);
        _histogramMock.Verify(histogram => histogram.GenerateHistogram(It.IsAny<List<List<int>>>()), Times.Once);
        _metadataMock.Verify(metadata => metadata.GenerateMedatadataFile(It.IsAny<List<List<int>>>()), Times.Once);
        _consoleHelperMock.Verify(helper => helper.ReadYKeyToProceed(It.IsAny<string>()), Times.Exactly(2));
        _fileHelperMock.Verify(helper => helper.WriteSettingsToFile(It.IsAny<bool>()), Times.Once);
        _consoleHelperMock.Verify(helper => helper.WriteSettingsSavedMessage(It.IsAny<bool>()), Times.Once);
    }
}