using System.Drawing;
using System.Reflection;
using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.App.Helpers;

public static class ColorHelper
{
    /// <summary>
    /// The default ANSI foreground colour.
    /// </summary>
    public static readonly string DefaultAnsiForegroundColour = HexColorToAnsiForegroundColorString(GetHexColor(AppColor.Gray.ToString())!);

    /// <summary>
    /// The default ANSI background colour.
    /// </summary>
    public static readonly string DefaultAnsiBackgroundColour = HexColorToAnsiBackgroundColorString(GetHexColor(AppColor.VsCodeGray.ToString())!);

    /// <summary>
    /// Get the hex color code for an AppColor name.
    /// </summary>
    /// <param name="colorName"></param>
    /// <returns></returns>
    public static string? GetHexColor(string colorName)
    {
        if (!Enum.TryParse<AppColor>(colorName, true, out var color))
        {
            return null;
        }

        FieldInfo? field = typeof(AppColor).GetField(color.ToString());
        var attribute = field?.GetCustomAttribute<HexColorAttribute>();

        return attribute?.HexValue ?? GetHexColor(AppColor.Gray.ToString());
    }

    /// <summary>
    /// Convert a hex color code to an ANSI foreground colour escape sequence.
    /// </summary>
    /// <param name="hexColor"></param>
    /// <returns></returns>
    public static string HexColorToAnsiForegroundColorString(string hexColor)
    {
        Color color = ColorTranslator.FromHtml(hexColor);

        return $"\x1b[1m\x1b[38;2;{color.R};{color.G};{color.B}m";
    }

    /// <summary>
    /// Convert a hex color code to an ANSI background colour escape sequence.
    /// </summary>
    /// <param name="hexColor"></param>
    /// <returns></returns>
    public static string HexColorToAnsiBackgroundColorString(string hexColor)
    {
        Color color = ColorTranslator.FromHtml(hexColor);

        return $"\x1b[1m\x1b[48;2;{color.R};{color.G};{color.B}m";
    }
}