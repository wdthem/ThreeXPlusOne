using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.ObjectModel;
using System.Drawing;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.DirectedGraph;
using ThreeXPlusOne.App.DirectedGraph.Shapes;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.UnitTests.Mocks;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class DirectedGraphTests
{
    private readonly Mock<IConsoleService> _consoleServiceMock;
    private readonly Mock<ILightSourceService> _lightSourceServiceMock;
    private readonly IEnumerable<IDirectedGraphDrawingService> _graphServicesList;
    private readonly Mock<IDirectedGraphDrawingService> _graphServiceMock;
    private readonly ShapeFactory _shapeFactory;
    private readonly IOptions<AppSettings> _appSettings = new OptionsWrapper<AppSettings>
    (
        new AppSettings { }
    );

    public DirectedGraphTests()
    {
        _consoleServiceMock = new Mock<IConsoleService>();
        _lightSourceServiceMock = new Mock<ILightSourceService>();
        _graphServiceMock = new Mock<IDirectedGraphDrawingService>();
        _graphServicesList = [_graphServiceMock.Object];
        _shapeFactory = new ShapeFactory([]);
    }

    [Fact]
    public void AddSeries_Success00()
    {
        // Arrange
        List<List<int>> seriesLists = [[64, 32, 16, 8, 4, 2, 1], [5, 16, 8, 4, 2, 1]];

        TwoDimensionalDirectedGraph twoDimensionalGraph = new(_appSettings,
                                                              _graphServicesList,
                                                              _lightSourceServiceMock.Object,
                                                              _consoleServiceMock.Object,
                                                              _shapeFactory);

        // Act + Assert
        twoDimensionalGraph.Invoking(graph => graph.AddSeries(seriesLists)).Should().NotThrow();
    }

    [Theory]

    //even node values
    [InlineData(236, 3657, 234, 0.8, 3659.9106805013857, 182.91749711792292)]
    [InlineData(236, -3657, 234, 0.8, -3659.9106805013857, 182.91749711792292)]
    [InlineData(236, 3657, -234, 0.8, 3659.9106805013857, -182.91749711792292)]

    //odd node values
    [InlineData(377, 5734, 749, 0.8, 5722.98339959533, 828.9861325476279)]
    [InlineData(377, -5734, 749, 0.8, -5722.98339959533, 828.9861325476279)]
    [InlineData(377, 5734, -749, 0.8, 5722.98339959533, -828.9861325476279)]
    public void RotateNode_Success00(int nodeValue,
                                     double xCoordinate,
                                     double yCoordinate,
                                     double rotationAngle,
                                     double expectedXPrime,
                                     double expectedYPrime)
    {
        // Arrange
        MockDirectedGraph mockDirectedGraph = new(_appSettings,
                                                  _graphServicesList,
                                                  _lightSourceServiceMock.Object,
                                                  _consoleServiceMock.Object,
                                                  _shapeFactory);

        // Act
        (double xPrime, double yPrime) = mockDirectedGraph.RotateNode_Base(nodeValue, rotationAngle, xCoordinate, yCoordinate);

        // Assert
        xPrime.Should().Be(expectedXPrime);
        yPrime.Should().Be(expectedYPrime);
    }

    /// <summary>
    /// Ensure all expected methods on the IGraphService interface are called (crucially, IGraphService.Dispose()).
    /// </summary>
    [Fact]
    public void DrawDirectedGraph_Success00()
    {
        // Arrange
        _graphServiceMock.Setup(service => service.GraphProvider).Returns(GraphProvider.SkiaSharp);
        _graphServiceMock.Setup(service => service.SupportedDimensions).Returns(new ReadOnlyCollection<int>([2]));

        MockDirectedGraph mockDirectedGraph = new(_appSettings,
                                                  _graphServicesList,
                                                  _lightSourceServiceMock.Object,
                                                  _consoleServiceMock.Object,
                                                  _shapeFactory);

        // Act
        mockDirectedGraph.DrawDirectedGraph_Base();

        // Assert
        _graphServiceMock.Verify(service => service.Initialize(It.IsAny<List<DirectedGraphNode>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Color>()), Times.Once);
        _graphServiceMock.Verify(service => service.GenerateBackgroundStars(It.IsAny<int>()), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.GenerateLightSource(It.IsAny<(double, double)>(), It.IsAny<double>(), It.IsAny<Color>()), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.Draw(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        _graphServiceMock.Verify(service => service.SaveImage(It.IsAny<string>()), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.Dispose(), Times.Once);
    }
}