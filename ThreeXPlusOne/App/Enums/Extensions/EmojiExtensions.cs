using System.Reflection;

namespace ThreeXPlusOne.App.Enums.Extensions;

public static class EmojiExtensions
{
    /// <summary>
    /// Get the Unicode value for an Emoji name.
    /// </summary>
    /// <param name="emojiName"></param>
    /// <returns></returns>
    public static string GetUnicodeValue(this Enum emojiName)
    {
        FieldInfo? field = typeof(Emoji).GetField(emojiName.ToString());
        var attribute = field?.GetCustomAttribute<EmojiUnicodeValueAttribute>();

        return attribute?.UnicodeValue ?? GetUnicodeValue(Emoji.QuestionMark);
    }
}