using System.Drawing;
using ThreeXPlusOne.Code.Enums;

namespace ThreeXPlusOne.Code.Interfaces;

public interface ILightSourceService
{
    LightSourcePosition LightSourcePosition { get; }

    /// <summary>
    /// The radius of the light source
    /// </summary>
    float Radius { get; }

    /// <summary>
    /// The color of the lightsource
    /// </summary>
    Color LightSourceColor { get; }

    /// <summary>
    /// Initialize the light source service with details about the graph being generated
    /// </summary>
    /// <param name="canvasWidth"></param>
    /// <param name="canvasHeight"></param>
    /// <param name="graphDimensions"></param>
    /// <param name="lightSourcePositionSetting"></param>
    void Initialize(int canvasWidth, int canvasHeight, int graphDimensions, string lightSourcePositionSetting);

    /// <summary>
    /// Get canvas coordinates for the light source based on where the user specified that it should exist
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    (float X, float Y) GetLightSourceCoordinates(LightSourcePosition position);
}