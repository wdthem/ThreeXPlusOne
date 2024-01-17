using System.Drawing;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Enums;

namespace ThreeXPlusOne.Code.Services;

public class LightSourceService() : ILightSourceService
{
    private (int Width, int Height) _canvasDimensions;
    private int _graphDimensions = 2;
    private Dictionary<LightSourcePosition, (float X, float Y)>? _positionMappings;

    /// <summary>
    /// The dimenions that the current graphs is being rendered in
    /// </summary>
    public int GraphDimensions
    {
        get => _graphDimensions;
        set
        {
            _graphDimensions = value;
        }
    }

    /// <summary>
    /// The dimensions of the canvas used to map to coordinates
    /// </summary>
    public (int Width, int Height) CanvasDimensions
    {
        get => _canvasDimensions;
        set
        {
            _canvasDimensions = value;
            InitializePositionMappings();
        }
    }

    /// <summary>
    /// The radius of the light source
    /// </summary>
    public float Radius
    {
        get
        {
            return (float)Math.Sqrt(Math.Pow(_canvasDimensions.Width, 2) + Math.Pow(_canvasDimensions.Height, 2));
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
    /// Parse the value from settings into a LightSourcePosition enum value
    /// </summary>
    /// <param name="settingsValue"></param>
    /// <returns></returns>
    public LightSourcePosition ParseLightSourcePosition(string settingsValue)
    {
        if (!Enum.TryParse(settingsValue, out LightSourcePosition position))
        {
            return LightSourcePosition.None;
        }

        return position;
    }

    /// <summary>
    /// Get canvas coordinates for the light source based on where the user specified that it should exist
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public (float X, float Y) GetLightSourceCoordinates(LightSourcePosition position)
    {
        if (_positionMappings != null &&
            _positionMappings.TryGetValue(position, out (float X, float Y) coordinates))
        {
            return coordinates;
        }
        else
        {
            throw new KeyNotFoundException($"Coordinates not found for the light source position '{position}'");
        }
    }

    /// <summary>
    /// Populate the dictionary mapping text descriptions of coordinates to x,y coordinates using the canvas dimensions
    /// </summary>
    private void InitializePositionMappings()
    {
        _positionMappings = new Dictionary<LightSourcePosition, (float X, float Y)>
            {
                { LightSourcePosition.None, (-1, -1) },

                { LightSourcePosition.TopLeft, (0, 0) },
                { LightSourcePosition.TopCenter, (_canvasDimensions.Width / 2, 0) },
                { LightSourcePosition.TopRight, (_canvasDimensions.Width, 0) },

                { LightSourcePosition.BottomLeft, (0, _canvasDimensions.Height) },
                { LightSourcePosition.BottomCenter, (_canvasDimensions.Width / 2, _canvasDimensions.Height) },
                { LightSourcePosition.BottomRight, (_canvasDimensions.Width, _canvasDimensions.Height) },

                { LightSourcePosition.LeftCenter, (0, _canvasDimensions.Height / 2) },
                { LightSourcePosition.RightCenter, (_canvasDimensions.Width, _canvasDimensions.Height / 2) }
            };
    }
}