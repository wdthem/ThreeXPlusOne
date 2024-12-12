using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Drawing;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.UnitTests.Mocks;
using Xunit;
using ThreeXPlusOne.App.DirectedGraph.GraphInstances;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.UnitTests.DirectedGraph;

public class DirectedGraphTests
{
    private readonly Mock<ILightSourceService> _lightSourceServiceMock;
    private readonly IEnumerable<IDirectedGraphDrawingService> _graphServicesList;
    private readonly Mock<IDirectedGraphDrawingService> _graphServiceMock;
    private readonly Mock<IDirectedGraphPresenter> _directedGraphPresenterMock;
    private readonly ShapeFactory _shapeFactory;
    private readonly IOptions<AppSettings> _appSettings = new OptionsWrapper<AppSettings>
    (
        new AppSettings { }
    );

    public DirectedGraphTests()
    {
        _lightSourceServiceMock = new Mock<ILightSourceService>();
        _graphServiceMock = new Mock<IDirectedGraphDrawingService>();
        _graphServicesList = [_graphServiceMock.Object];
        _shapeFactory = new ShapeFactory([]);
        _directedGraphPresenterMock = new Mock<IDirectedGraphPresenter>();
    }

    [Fact]
    public void AddSeries_Success00()
    {
        // Arrange
        List<CollatzResult> collatzResults = [new CollatzResult(){Values = [64, 32, 16, 8, 4, 2, 1]},
                                              new CollatzResult(){Values = [5, 16, 8, 4, 2, 1]}];

        Standard2DDirectedGraph twoDimensionalGraph = new(_appSettings,
                                                          _graphServicesList,
                                                          _lightSourceServiceMock.Object,
                                                          _shapeFactory,
                                                          _directedGraphPresenterMock.Object);

        // Act + Assert
        twoDimensionalGraph.Invoking(graph => graph.AddSeries(GraphType.Standard2D, collatzResults)).Should().NotThrow();
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
                                                  _shapeFactory,
                                                  _directedGraphPresenterMock.Object);

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
    public async Task DrawDirectedGraph_Success00()
    {
        // Arrange
        _graphServiceMock.Setup(service => service.GraphProvider).Returns(GraphProvider.SkiaSharp);

        MockDirectedGraph mockDirectedGraph = new(_appSettings,
                                                  _graphServicesList,
                                                  _lightSourceServiceMock.Object,
                                                  _shapeFactory,
                                                  _directedGraphPresenterMock.Object);

        // Act
        await mockDirectedGraph.DrawDirectedGraph_Base();

        // Assert
        _graphServiceMock.Verify(service => service.Initialize(It.IsAny<List<DirectedGraphNode>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Color>()), Times.Once);
        _graphServiceMock.Verify(service => service.GenerateLightSource(It.IsAny<(double, double)>(), It.IsAny<double>(), It.IsAny<Color>()), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.Draw(It.IsAny<bool>(), It.IsAny<bool>()), Times.Once);
        _graphServiceMock.Verify(service => service.SaveImage(It.IsAny<string>(), It.IsAny<int>()), Times.AtMost(1));
        _graphServiceMock.Verify(service => service.Dispose(), Times.Once);
    }

    /// <summary>
    /// Ensure that the nodes are translated to positive coordinates.
    /// </summary>
    [Fact]
    public void TranslateNodesToPositiveCoordinates_Success00()
    {
        // Arrange
        MockDirectedGraph mockDirectedGraph = new(_appSettings,
                                                  _graphServicesList,
                                                  _lightSourceServiceMock.Object,
                                                  _shapeFactory,
                                                  _directedGraphPresenterMock.Object);

        Dictionary<int, DirectedGraphNode> nodes = new()
        {
            {1, new DirectedGraphNode(1){Position = (-345, -434)}},
            {2, new DirectedGraphNode(2){Position = (-5, -15)}},
            {3, new DirectedGraphNode(2){Position = (100, 24)}}
        };

        double xNodeSpacer = 125;
        double yNodeSpacer = 125;
        double nodeRadius = 50;

        // Act
        mockDirectedGraph.TranslateNodesToPositiveCoordinates_Base(nodes, xNodeSpacer, yNodeSpacer, nodeRadius);

        // Assert
        nodes[1].Position.Should().Be((175, 175));
        nodes[2].Position.Should().Be((515, 594));
        nodes[3].Position.Should().Be((620, 633));
    }
}