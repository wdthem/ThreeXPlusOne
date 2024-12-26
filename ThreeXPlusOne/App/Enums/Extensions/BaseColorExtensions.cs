using System.Reflection;

namespace ThreeXPlusOne.App.Enums.Extensions;

public static class BaseColorExtensions
{
    /// <summary>
    /// Convert the BaseColor to an ANSI code.
    /// </summary>
    /// <param name="baseColor"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string ToAnsiCode(this BaseColor baseColor)
    {
        var field = typeof(BaseColor).GetField(baseColor.ToString());
        var attribute = (field?.GetCustomAttribute<AppColorValueAttribute>())
            ?? throw new InvalidOperationException($"No AppColorValue attribute found for {baseColor}");

        return baseColor switch
        {
            BaseColor.Foreground => attribute.AppColor.ToAnsiForegroundCode(),
            BaseColor.Background => attribute.AppColor.ToAnsiBackgroundCode(),
            _ => throw new ArgumentOutOfRangeException(nameof(baseColor))
        };
    }
}