using Microsoft.Extensions.Logging;
using Moq;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Services;
using ThreeXPlusOne.App.Services.Interfaces;
using Xunit;

namespace ThreeXPlusOne.UnitTests;

public class AppServiceTests
{
    private readonly Mock<IDirectedGraphService> _directedGraphServiceMock;
    private readonly Mock<IAppSettingsService> _appSettingsServiceMock;
    private readonly Mock<ILogger<AppService>> _loggerMock;
    private readonly Mock<IAppPresenter> _appPresenterMock;
    public AppServiceTests()
    {
        _directedGraphServiceMock = new Mock<IDirectedGraphService>();
        _appSettingsServiceMock = new Mock<IAppSettingsService>();
        _loggerMock = new Mock<ILogger<AppService>>();
        _appPresenterMock = new Mock<IAppPresenter>();
    }

    [Fact]
    public async Task Run_Success00()
    {
        // Arrange
        var process = new AppService(_loggerMock.Object,
                                     _directedGraphServiceMock.Object,
                                     _appSettingsServiceMock.Object,
                                     _appPresenterMock.Object);

        // Act
        await process.Run([]);

        // Assert
        _directedGraphServiceMock.Verify(graph => graph.GenerateDirectedGraph(), Times.Once);
        _appSettingsServiceMock.Verify(helper => helper.UpdateAppSettingsFile(), Times.Once);
    }
}