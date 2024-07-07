using System.Drawing;
using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.App.Interfaces.Services;

public interface ILightSourceService : IScopedService
{
    /// <summary>
    /// The enum describing the origin position of the light source.
    /// </summary>
    LightSourcePosition LightSourcePosition { get; }

    /// <summary>
    /// The radius of the light source.
    /// </summary>
    double Radius { get; }

    /// <summary>
    /// The color of the lightsource.
    /// </summary>
    Color LightSourceColor { get; }

    /// <summary>
    /// Initialize the light source service with details about the graph being generated.
    /// </summary>
    /// <param name="canvasWidth"></param>
    /// <param name="canvasHeight"></param>
    /// <param name="lightSourcePositionSetting"></param>
    /// <param name="lightSourceColor"></param>
    void Initialize(int canvasWidth, int canvasHeight, string lightSourcePositionSetting, string lightSourceColor);

    /// <summary>
    /// Get canvas coordinates for the light source based on where the user specified that it should exist.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    (double X, double Y) GetLightSourceCoordinates(LightSourcePosition position);

    /// <summary>
    /// Get the max distance of the effect of the light source.
    /// </summary>
    /// <returns></returns>
    double GetLightSourceMaxDistanceOfEffect();
}