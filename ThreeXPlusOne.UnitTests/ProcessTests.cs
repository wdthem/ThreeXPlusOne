using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.App;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.Services;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class ProcessTests
{
    private IOptions<AppSettings> _appSettings = new OptionsWrapper<AppSettings>
    (
        new AppSettings { }
    );

    private readonly Mock<IDirectedGraphService> _directedGraphServiceMock;
    private readonly Mock<IConsoleService> _consoleServiceMock;
    private readonly Mock<IAppSettingsService> _appSettingsServiceMock;
    private readonly Mock<ILogger<Process>> _loggerMock;
    public ProcessTests()
    {
        _directedGraphServiceMock = new Mock<IDirectedGraphService>();
        _consoleServiceMock = new Mock<IConsoleService>();
        _appSettingsServiceMock = new Mock<IAppSettingsService>();
        _loggerMock = new Mock<ILogger<Process>>();
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

        _consoleServiceMock.Setup(consoleService => consoleService.ReadYKeyToProceed("Generate Standard2D visualization?")).Returns(true);

        var process = new App.Process(_loggerMock.Object,
                                      _directedGraphServiceMock.Object,
                                      _appSettingsServiceMock.Object,
                                      _consoleServiceMock.Object);

        // Act
        await process.Run([]);

        // Assert
        _directedGraphServiceMock.Verify(graph => graph.GenerateDirectedGraph(), Times.Once);
        _appSettingsServiceMock.Verify(helper => helper.SaveGeneratedNumbers(), Times.Once);
    }
}