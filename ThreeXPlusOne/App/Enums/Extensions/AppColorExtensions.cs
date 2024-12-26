using System.Drawing;

namespace ThreeXPlusOne.App.Enums.Extensions;

public static class AppColorExtensions
{
    /// <summary>
    /// Get the hex code for an AppColor name.
    /// </summary>
    /// <param name="appColor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static string GetHexCode(this AppColor appColor)
    {
        var attribute = (HexCodeAttribute?)Attribute.GetCustomAttribute(
            appColor.GetType().GetField(appColor.ToString())!,
            typeof(HexCodeAttribute))
                ?? throw new InvalidOperationException($"No hex color found for {appColor}");

        return attribute.HexCode;
    }

    /// <summary>
    /// Get the Color from an AppColor name.
    /// </summary>
    /// <param name="appColor"></param>
    /// <param name="fallbackColor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private static Color GetColorFromAppColor(AppColor appColor,
                                              AppColor fallbackColor)
    {
        return ColorTranslator.FromHtml(appColor.GetHexCode()) is Color color && color != Color.Empty
                                            ? color
                                            : ColorTranslator.FromHtml(fallbackColor.GetHexCode());
    }

    /// <summary>
    /// Convert the AppColor to an ANSI foreground color code.
    /// </summary>
    /// <param name="appColor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string ToAnsiForegroundCode(this AppColor appColor)
    {
        Color color = GetColorFromAppColor(appColor, AppColor.Gray);

        return AnsiCode.ForegroundColor.GetCode(color.R, color.G, color.B);
    }

    /// <summary>
    /// Convert the AppColor to an ANSI background color code.
    /// </summary>
    /// <param name="appColor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string ToAnsiBackgroundCode(this AppColor appColor)
    {
        Color color = GetColorFromAppColor(appColor, AppColor.VsCodeGray);

        return AnsiCode.BackgroundColor.GetCode(color.R, color.G, color.B);
    }
}