using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services;
using Xunit;

namespace ThreeXPlusOne.UnitTests.Services;

public class FileServiceTests
{
    private readonly Mock<IUiComponent> _uiComponentMock;
    private IOptions<AppSettings> _appSettings = new OptionsWrapper<AppSettings>
    (
        new AppSettings { }
    );

    public FileServiceTests()
    {
        _uiComponentMock = new Mock<IUiComponent>();
    }

    private void ResetSettings()
    {
        _appSettings = new OptionsWrapper<AppSettings>
        (
            new AppSettings { }
        );

        _appSettings.Value.DirectedGraphAestheticSettings.GraphType = "Standard2D";
        _appSettings.Value.AlgorithmSettings.NumbersToUse = "1,2,3,4,5,6,7,8,9,10";
        _appSettings.Value.OutputPath = "";
    }

    [Fact]
    public void GenerateDirectedGraphFilePath_Success00()
    {
        // Arrange
        ResetSettings();
        var fileService = new FileService(_appSettings, _uiComponentMock.Object);

        // Act  
        var result = fileService.GenerateDirectedGraphFilePath(ImageType.Jpeg);

        // Assert
        result.Should().MatchRegex(@"ThreeXPlusOne-432f45b44c432414d2f97df0e5743818/ThreeXPlusOne-Standard2D-DirectedGraph-\d{8}-\d{6}.jpeg");
    }

    [Fact]
    public void GenerateHistogramFilePath_Success00()
    {
        // Arrange
        ResetSettings();
        var fileService = new FileService(_appSettings, _uiComponentMock.Object);

        // Act  
        var result = fileService.GenerateHistogramFilePath();

        // Assert
        result.Should().Be("ThreeXPlusOne-432f45b44c432414d2f97df0e5743818/ThreeXPlusOne-Histogram.png");
    }

    [Fact]
    public void GenerateMetadataFilePath_Success00()
    {
        // Arrange
        ResetSettings();
        var fileService = new FileService(_appSettings, _uiComponentMock.Object);

        // Act  
        var result = fileService.GenerateMetadataFilePath();

        // Assert
        result.Should().Be("ThreeXPlusOne-432f45b44c432414d2f97df0e5743818/ThreeXPlusOne-Metadata.txt");
    }
}