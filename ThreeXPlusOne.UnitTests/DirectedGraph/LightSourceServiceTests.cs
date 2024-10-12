using System.Drawing;
using FluentAssertions;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Services;
using Xunit;

namespace ThreeXPlusOne.UnitTests.DirectedGraph;

public class LightSourceServiceTests
{
    private readonly LightSourceService _lightSourceService;

    public LightSourceServiceTests()
    {
        _lightSourceService = new LightSourceService();
    }

    [Fact]
    public void Initialize_ValidParameters_SetsPropertiesCorrectly()
    {
        // Arrange
        int canvasWidth = 800;
        int canvasHeight = 600;
        string lightSourcePositionSetting = "TopCenter";
        string lightSourceColor = "#FF5733"; // Example hex color

        // Act
        _lightSourceService.Initialize(canvasWidth, canvasHeight, lightSourcePositionSetting, lightSourceColor);

        // Assert
        _lightSourceService.LightSourcePosition.Should().Be(LightSourcePosition.TopCenter);
        _lightSourceService.LightSourceColor.Should().NotBe(Color.LightYellow); // Should be the color set from hex
        _lightSourceService.Radius.Should().BeApproximately(1000, 0.1); // Check radius calculation
    }

    [Fact]
    public void GetLightSourceCoordinates_ValidPosition_ReturnsCoordinates()
    {
        // Arrange
        _lightSourceService.Initialize(800, 600, "TopCenter", "#FF5733");

        // Act
        var coordinates = _lightSourceService.GetLightSourceCoordinates(LightSourcePosition.TopCenter);

        // Assert
        coordinates.Should().Be((400, 0)); // Expected coordinates for TopCenter
    }

    [Fact]
    public void GetLightSourceCoordinates_InvalidPosition_ThrowsKeyNotFoundException()
    {
        // Arrange
        _lightSourceService.Initialize(800, 600, "TopCenter", "#FF5733");

        // Act
        Action act = () => _lightSourceService.GetLightSourceCoordinates((LightSourcePosition)99);

        // Assert
        act.Should().Throw<KeyNotFoundException>()
            .WithMessage("Coordinates not found for the light source position '99'");
    }

    [Fact]
    public void GetLightSourceMaxDistanceOfEffect_ReturnsCorrectHeight()
    {
        // Arrange
        int canvasWidth = 800;
        int canvasHeight = 600;
        _lightSourceService.Initialize(canvasWidth, canvasHeight, "TopCenter", "#FF5733");

        // Act
        var maxDistance = _lightSourceService.GetLightSourceMaxDistanceOfEffect();

        // Assert
        maxDistance.Should().Be(canvasHeight); // Should return the height of the canvas
    }
}