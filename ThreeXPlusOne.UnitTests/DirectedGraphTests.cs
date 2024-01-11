using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.UnitTests.Mocks;
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

    [Theory]

    //even node values
    [InlineData(236, 3657, 234, 0.8, 3659.9106, 182.9175)]
    [InlineData(236, -3657, 234, 0.8, -3659.9106, 182.9175)]
    [InlineData(236, 3657, -234, 0.8, 3659.9106, -182.9175)]

    //odd node values
    [InlineData(377, 5734, 749, 0.8, 5722.9834, 828.98615)]
    [InlineData(377, -5734, 749, 0.8, -5722.9834, 828.98615)]
    [InlineData(377, 5734, -749, 0.8, 5722.9834, -828.98615)]
    public void RotateNode_Success00(int nodeValue,
                                     float xCoordinate,
                                     float yCoordinate,
                                     float rotationAngle,
                                     float expectedXPrime,
                                     float expectedYPrime)
    {
        // Arrange

        MockDirectedGraph mockDirectedGraph = new(_settings,
                                                  _graphServicesList,
                                                  _consoleHelperMock.Object);

        // Act
        (float xPrime, float yPrime) = mockDirectedGraph.RotateNode_Base(nodeValue, rotationAngle, xCoordinate, yCoordinate);

        // Assert
        xPrime.Should().Be(expectedXPrime);
        yPrime.Should().Be(expectedYPrime);
    }
}