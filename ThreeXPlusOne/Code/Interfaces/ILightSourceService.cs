using System.Drawing;
using ThreeXPlusOne.Enums;

namespace ThreeXPlusOne.Code.Interfaces;

public interface ILightSourceService
{
    /// <summary>
    /// The dimenions that the current graphs is being rendered in
    /// </summary>
    int GraphDimensions { get; set; }

    /// <summary>
    /// The dimensions of the canvas used to map to coordinates
    /// </summary>
    (int Width, int Height) CanvasDimensions { get; set; }

    /// <summary>
    /// The radius of the light source
    /// </summary>
    float Radius { get; }

    /// <summary>
    /// The color of the lightsource
    /// </summary>
    Color LightSourceColor { get; }

    /// <summary>
    /// Parse the value from settings into a LightSourcePosition enum value
    /// </summary>
    /// <param name="settingsValue"></param>
    /// <returns></returns>
    LightSourcePosition ParseLightSourcePosition(string input);

    /// <summary>
    /// Get canvas coordinates for the light source based on where the user specified that it should exist
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    (float X, float Y) GetLightSourceCoordinates(LightSourcePosition position);
}