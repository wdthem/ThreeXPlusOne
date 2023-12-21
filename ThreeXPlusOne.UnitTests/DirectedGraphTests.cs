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
    private readonly Mock<IFileHelper> _fileHelperMock;
    private readonly Mock<IConsoleHelper> _consoleHelperMock;
    private readonly IOptions<Settings> _settings = new OptionsWrapper<Settings>
    (
        new Settings { }
    );

    public DirectedGraphTests()
    {
        _fileHelperMock = new Mock<IFileHelper>();
        _consoleHelperMock = new Mock<IConsoleHelper>();
    }

    [Fact]
    public void AddSeries_Success00()
    {
        // Arrange
        List<int> series = [64, 32, 16, 8, 4, 2, 1];

        TwoDimensionalDirectedGraph twoDimensionalGraph = new(_settings, _fileHelperMock.Object, _consoleHelperMock.Object);

        // Act + Assert
        twoDimensionalGraph.Invoking(graph => graph.AddSeries(series)).Should().NotThrow();
    }
}