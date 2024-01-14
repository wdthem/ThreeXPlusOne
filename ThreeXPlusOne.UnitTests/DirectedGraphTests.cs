using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.ObjectModel;
using ThreeXPlusOne.Code.Graph;
using ThreeXPlusOne.Code.Graph.Services;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Models;
using ThreeXPlusOne.UnitTests.Mocks;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class DirectedGraphTests
{
    private readonly Mock<IConsoleHelper> _consoleHelperMock;
    private readonly IEnumerable<IGraphService> _graphServicesList;
    private readonly Mock<IGraphService> _graphServiceMock;
    private readonly IOptions<Settings> _settings = new OptionsWrapper<Settings>
    (
        new Settings { }
    );

    public DirectedGraphTests()
    {
        _consoleHelperMock = new Mock<IConsoleHelper>();
        _graphServiceMock = new Mock<IGraphService>();
        _graphServicesList = [_graphServiceMock.Object];
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

    /// <summary>
    /// Ensure all expected methods on the IGraphService interface are called (crucially, IGraphService.Dispose())
    /// </summary>
    [Fact]
    public void DrawDirectedGraph_Success00()
    {
        // Arrange
        _graphServiceMock.Setup(service => service.GraphProvider).Returns(GraphProvider.SkiaSharp);
        _graphServiceMock.Setup(service => service.SupportedDimensions).Returns(new ReadOnlyCollection<int>([2]));

        MockDirectedGraph mockDirectedGraph = new(_settings,
                                                  _graphServicesList,
                                                  _consoleHelperMock.Object);

        // Act
        mockDirectedGraph.DrawDirectedGraph_Base();

        // Assert
        _graphServiceMock.Verify(service => service.Initialize(It.IsAny<List<DirectedGraphNode>>(), It.IsAny<int>(), It.IsAny<int>()), Times.Once);
        _graphServiceMock.Verify(service => service.GenerateBackgroundStars(It.IsAny<int>()), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.GenerateLightSource(), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.Draw(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        _graphServiceMock.Verify(service => service.Render(), Times.Once);
        _graphServiceMock.Verify(service => service.SaveImage(), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.Dispose(), Times.Once);
    }
}