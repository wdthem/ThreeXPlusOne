namespace ThreeXPlusOne.App.Enums;

/// <summary>
/// Emoji enum.
/// </summary>
public enum Emoji
{
    /// <summary>
    /// 😊 smile
    /// </summary>
    [EmojiUnicodeValue("\uD83D\uDE00")]
    Smile,

    /// <summary>
    /// ✅ check mark
    /// </summary>
    [EmojiUnicodeValue("\u2705")]
    GreenCheckMark,

    /// <summary>
    /// 📄 document
    /// </summary>
    [EmojiUnicodeValue("\uD83D\uDCC4")]
    Document,

    /// <summary>
    /// ❌ red X
    /// </summary>
    [EmojiUnicodeValue("\u274C")]
    RedX,

    /// <summary>
    /// 🔒 locked padlock
    /// </summary>
    [EmojiUnicodeValue("\uD83D\uDD12")]
    Lock,

    /// <summary>
    /// 🔎 magnifying glass
    /// </summary>
    [EmojiUnicodeValue("\uD83D\uDD0E")]
    MagnifyingGlass,

    /// <summary>
    /// 🖼️ framed picture
    /// </summary>
    [EmojiUnicodeValue("\uD83D\uDDBC\uFE0F")]
    Picture,

    /// <summary>
    /// ❓ question mark
    /// </summary>
    [EmojiUnicodeValue("\u2753")]
    QuestionMark,

    /// <summary>
    /// 🤔 thinking face
    /// </summary>
    [EmojiUnicodeValue("\uD83E\uDD14")]
    ThinkingFace,

    // MOON SPINNER EMOJIS
    // *******************

    /// <summary>
    /// 🌑 new moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF11")]
    NewMoon,

    /// <summary>
    /// 🌒 waxing crescent moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF12")]
    WaxingCrescentMoon,

    /// <summary>
    /// 🌓 first quarter moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF13")]
    FirstQuarterMoon,

    /// <summary>
    /// 🌔 waxing gibbous moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF14")]
    WaxingGibbousMoon,

    /// <summary>
    /// 🌕 full moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF15")]
    FullMoon,

    /// <summary>
    /// 🌖 waning gibbous moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF16")]
    WaningGibbousMoon,

    /// <summary>
    /// 🌗 last quarter moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF17")]
    LastQuarterMoon,

    /// <summary>
    /// 🌘 waning crescent moon
    /// </summary>
    [EmojiUnicodeValue("\uD83C\uDF18")]
    WaningCrescentMoon
}

[AttributeUsage(AttributeTargets.Field)]
public class EmojiUnicodeValueAttribute(string ansiValue) : Attribute
{
    public string AnsiValue { get; } = ansiValue;
}