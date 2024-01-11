using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class DirectedGraphTests
{
    private readonly Mock<IConsoleHelper> _consoleHelperMock;
    private readonly IEnumerable<IGraphService> _graphServicesList;
    private readonly IOptions<Settings> _settings = new OptionsWrapper<Settings>
    (
        new Settings { }
    );

    public DirectedGraphTests()
    {
        _consoleHelperMock = new Mock<IConsoleHelper>();
        _graphServicesList = [new Mock<IGraphService>().Object];
    }

    [Fact]
    public void AddSeries_Success00()
    {
        // Arrange
        List<List<int>> seriesLists = [[64, 32, 16, 8, 4, 2, 1], [5, 16, 8, 4, 2, 1]];

        TwoDimensionalDirectedGraph twoDimensionalGraph = new(_settings,
                                                              _graphServicesList,
                                                              _consoleHelperMock.Object);

        // Act + Assert
        twoDimensionalGraph.Invoking(graph => graph.AddSeries(seriesLists)).Should().NotThrow();
    }
}