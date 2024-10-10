using System.Diagnostics;
using Microsoft.Extensions.Options;
using Moq;
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

    private readonly Mock<IAlgorithmService> _algorithmServiceMock;
    private readonly Mock<IDirectedGraphService> _directedGraphServiceMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IConsoleService> _consoleServiceMock;

    public ProcessTests()
    {
        _algorithmServiceMock = new Mock<IAlgorithmService>();
        _directedGraphServiceMock = new Mock<IDirectedGraphService>();
        _fileServiceMock = new Mock<IFileService>();
        _consoleServiceMock = new Mock<IConsoleService>();
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

        var process = new App.Process(_appSettings,
                                      _directedGraphServiceMock.Object,
                                      _fileServiceMock.Object,
                                      _consoleServiceMock.Object);

        // Act
        await process.Run([]);

        // Assert
        _directedGraphServiceMock.Verify(graph => graph.GenerateDirectedGraph(It.IsAny<Stopwatch>()), Times.Once);
        _fileServiceMock.Verify(helper => helper.WriteSettingsToFile(It.IsAny<bool>()), Times.Once);
        _consoleServiceMock.Verify(helper => helper.WriteSettingsSavedMessage(It.IsAny<bool>()), Times.Once);
    }
}