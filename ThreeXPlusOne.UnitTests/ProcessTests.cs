using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ThreeXPlusOne.App;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Services;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class ProcessTests
{
    private readonly Mock<IDirectedGraphService> _directedGraphServiceMock;
    private readonly Mock<IConsoleService> _consoleServiceMock;
    private readonly Mock<IAppSettingsService> _appSettingsServiceMock;
    private readonly Mock<ILogger<AppService>> _loggerMock;

    public ProcessTests()
    {
        _directedGraphServiceMock = new Mock<IDirectedGraphService>();
        _consoleServiceMock = new Mock<IConsoleService>();
        _appSettingsServiceMock = new Mock<IAppSettingsService>();
        _loggerMock = new Mock<ILogger<AppService>>();
    }

    [Fact]
    public async Task Run_Success00()
    {
        // Arrange
        _consoleServiceMock.Setup(consoleService => consoleService.AskForConfirmation("Generate Standard2D visualisation?")).Returns(true);

        var process = new AppService(_loggerMock.Object,
                                     _directedGraphServiceMock.Object,
                                     _appSettingsServiceMock.Object,
                                     _consoleServiceMock.Object);

        // Act
        await process.Run([]);

        // Assert
        _directedGraphServiceMock.Verify(graph => graph.GenerateDirectedGraph(), Times.Once);
        _appSettingsServiceMock.Verify(helper => helper.UpdateAppSettingsFile(), Times.Once);
    }
}