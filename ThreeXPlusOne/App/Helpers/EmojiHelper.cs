using System.Reflection;
using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.App.Helpers;

public static class EmojiHelper
{
    /// <summary>
    /// Get the hex color code for an AppColor name.
    /// </summary>
    /// <param name="colorName"></param>
    /// <returns></returns>
    public static string GetEmojiUnicodeValue(Enum emojiName)
    {
        FieldInfo? field = typeof(Emoji).GetField(emojiName.ToString());
        var attribute = field?.GetCustomAttribute<EmojiUnicodeValueAttribute>();

        return attribute?.UnicodeValue ?? GetEmojiUnicodeValue(Emoji.RedX);
    }
}