using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public class LightSourceService() : ILightSourceService
{
    private (int Width, int Height) _canvasDimensions;
    private LightSourcePosition _lightSourcePosition = LightSourcePosition.None;
    private Color _lightSourceColor = Color.FromArgb(200, 255, 255, 224); // LightYellow with an alpha of 200
    private Dictionary<LightSourcePosition, (double X, double Y)>? _positionMappings;

    /// <summary>
    /// The position of the light source.
    /// </summary>
    public LightSourcePosition LightSourcePosition
    {
        get
        {
            return _lightSourcePosition;
        }
    }

    /// <summary>
    /// The radius of the light source.
    /// </summary>
    public double Radius
    {
        get
        {
            return Math.Sqrt(Math.Pow(_canvasDimensions.Width, 2) + Math.Pow(_canvasDimensions.Height, 2));
        }
    }

    /// <summary>
    /// The color of the lightsource.
    /// </summary>
    public Color LightSourceColor
    {
        get
        {
            return _lightSourceColor;
        }
    }

    /// <summary>
    /// Initialize the light source service with details about the graph being generated.
    /// </summary>
    /// <param name="canvasWidth"></param>
    /// <param name="canvasHeight"></param>
    /// <param name="lightSourcePositionSetting"></param>
    /// <param name="lightSourceColor"></param>
    public void Initialize(int canvasWidth,
                           int canvasHeight,
                           string lightSourcePositionSetting,
                           string lightSourceColor)
    {
        _canvasDimensions = (canvasWidth, canvasHeight);
        _lightSourcePosition = ParseLightSourcePosition(lightSourcePositionSetting);
        _lightSourceColor = SetLightSourceColor(lightSourceColor);

        InitializePositionMappings();
    }

    /// <summary>
    /// Get canvas coordinates for the light source based on where the user specified that it should exist.
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public (double X, double Y) GetLightSourceCoordinates(LightSourcePosition position)
    {
        if (_positionMappings != null &&
            _positionMappings.TryGetValue(position, out (double X, double Y) coordinates))
        {
            return coordinates;
        }
        else
        {
            throw new KeyNotFoundException($"Coordinates not found for the light source position '{position}'");
        }
    }

    /// <summary>
    /// Get the max distance of the effect of the light source.
    /// </summary>
    /// <returns></returns>
    public double GetLightSourceMaxDistanceOfEffect()
    {
        return _lightSourcePosition switch
        {
            LightSourcePosition.LeftCenter or LightSourcePosition.RightCenter => _canvasDimensions.Width,
            LightSourcePosition.TopCenter or LightSourcePosition.BottomCenter => _canvasDimensions.Height,
            LightSourcePosition.TopLeft or
            LightSourcePosition.BottomLeft or
            LightSourcePosition.TopRight or
            LightSourcePosition.BottomRight => Math.Sqrt(Math.Pow(_canvasDimensions.Width, 2) + Math.Pow(_canvasDimensions.Height, 2)),
            _ => 0,// Default to 0 for any other position
        };
    }

    /// <summary>
    /// Set the light source colour based on app settings.
    /// </summary>
    /// <param name="hexCode"></param>
    private Color SetLightSourceColor(string hexCode)
    {
        if (string.IsNullOrWhiteSpace(hexCode))
        {
            return _lightSourceColor;
        }

        Color colorFromHexCode = ColorTranslator.FromHtml(hexCode);

        if (colorFromHexCode == Color.Empty)
        {
            return _lightSourceColor;
        }

        return Color.FromArgb(200,
                              colorFromHexCode.R,
                              colorFromHexCode.G,
                              colorFromHexCode.B);
    }

    /// <summary>
    /// Parse the value from appSettings into a LightSourcePosition enum value.
    /// </summary>
    /// <param name="appSettingsValue"></param>
    /// <returns></returns>
    private static LightSourcePosition ParseLightSourcePosition(string appSettingsValue)
    {
        if (!Enum.TryParse(appSettingsValue, out LightSourcePosition position))
        {
            return LightSourcePosition.None;
        }

        return position;
    }

    /// <summary>
    /// Populate the dictionary mapping text descriptions of coordinates to x,y coordinates using the canvas dimensions.
    /// </summary>
    private void InitializePositionMappings()
    {
        _positionMappings = new Dictionary<LightSourcePosition, (double X, double Y)>
        {
            { LightSourcePosition.None, (-1, -1) },

            { LightSourcePosition.TopLeft, (0, 0) },
            { LightSourcePosition.TopCenter, (_canvasDimensions.Width / 2, 0) },
            { LightSourcePosition.TopRight, (_canvasDimensions.Width, 0) },

            { LightSourcePosition.BottomLeft, (0, _canvasDimensions.Height)},
            { LightSourcePosition.BottomCenter, (_canvasDimensions.Width / 2, _canvasDimensions.Height) },
            { LightSourcePosition.BottomRight, (_canvasDimensions.Width, _canvasDimensions.Height) },

            { LightSourcePosition.LeftCenter, (0, _canvasDimensions.Height / 2) },
            { LightSourcePosition.RightCenter, (_canvasDimensions.Width, _canvasDimensions.Height / 2) }
        };
    }
}