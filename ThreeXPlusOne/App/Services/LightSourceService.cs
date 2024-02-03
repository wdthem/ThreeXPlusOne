using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public class LightSourceService() : ILightSourceService
{
    private (int Width, int Height) _canvasDimensions;
    private int _graphDimensions;
    private LightSourcePosition _lightSourcePosition = LightSourcePosition.None;
    private Dictionary<LightSourcePosition, (double X, double Y)>? _positionMappings;

    /// <summary>
    /// The position of the light source
    /// </summary>
    public LightSourcePosition LightSourcePosition
    {
        get
        {
            return _lightSourcePosition;
        }
    }

    /// <summary>
    /// The radius of the light source
    /// </summary>
    public double Radius
    {
        get
        {
            return Math.Sqrt(Math.Pow(_canvasDimensions.Width, 2) + Math.Pow(_canvasDimensions.Height, 2));
        }
    }

    /// <summary>
    /// The color of the lightsource
    /// </summary>
    public Color LightSourceColor
    {
        get
        {
            return Color.FromArgb(200,
                                  Color.LightYellow.R,
                                  Color.LightYellow.G,
                                  Color.LightYellow.B);
        }
    }

    /// <summary>
    /// Initialize the light source service with details about the graph being generated
    /// </summary>
    /// <param name="canvasWidth"></param>
    /// <param name="canvasHeight"></param>
    /// <param name="graphDimensions"></param>
    /// <param name="lightSourcePositionSetting"></param>
    public void Initialize(int canvasWidth,
                           int canvasHeight,
                           int graphDimensions,
                           string lightSourcePositionSetting)
    {
        _canvasDimensions = (canvasWidth, canvasHeight);
        _graphDimensions = graphDimensions;
        _lightSourcePosition = ParseLightSourcePosition(lightSourcePositionSetting);

        InitializePositionMappings();
    }

    /// <summary>
    /// Get canvas coordinates for the light source based on where the user specified that it should exist
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
    /// Get the max distance of the effect of the light source
    /// </summary>
    /// <returns></returns>
    public double GetLightSourceMaxDistanceOfEffect()
    {
        if (_graphDimensions == 3)
        {
            return _canvasDimensions.Height / 1.2;
        }

        return _canvasDimensions.Height / 2.0;
    }

    /// <summary>
    /// Parse the value from settings into a LightSourcePosition enum value
    /// </summary>
    /// <param name="settingsValue"></param>
    /// <returns></returns>
    private static LightSourcePosition ParseLightSourcePosition(string settingsValue)
    {
        if (!Enum.TryParse(settingsValue, out LightSourcePosition position))
        {
            return LightSourcePosition.None;
        }

        return position;
    }

    /// <summary>
    /// Populate the dictionary mapping text descriptions of coordinates to x,y coordinates using the canvas dimensions
    /// </summary>
    private void InitializePositionMappings()
    {
        double halfWidth = _canvasDimensions.Width / 2;
        double halfHeight = _canvasDimensions.Height / 2;

        _positionMappings = new Dictionary<LightSourcePosition, (double X, double Y)>
        {
            { LightSourcePosition.None, (-1, -1) },

            { LightSourcePosition.TopLeft,
                _graphDimensions == 2 ? (-halfWidth, -halfHeight) : (0, 0) },
            { LightSourcePosition.TopCenter,
                _graphDimensions == 2 ? (_canvasDimensions.Width / 2, -halfHeight) : (_canvasDimensions.Width / 2, 0) },
            { LightSourcePosition.TopRight,
                _graphDimensions == 2 ? (_canvasDimensions.Width + halfWidth, -halfHeight) : (_canvasDimensions.Width, 0) },

            { LightSourcePosition.BottomLeft, (-halfWidth, _canvasDimensions.Height + halfHeight)},
            { LightSourcePosition.BottomCenter, (_canvasDimensions.Width / 2, _canvasDimensions.Height + halfHeight) },
            { LightSourcePosition.BottomRight, (_canvasDimensions.Width + halfWidth, _canvasDimensions.Height + halfHeight) },

            { LightSourcePosition.LeftCenter, (-halfWidth, _canvasDimensions.Height / 2) },
            { LightSourcePosition.RightCenter, (_canvasDimensions.Width + halfWidth, _canvasDimensions.Height / 2) }
        };

    }
}