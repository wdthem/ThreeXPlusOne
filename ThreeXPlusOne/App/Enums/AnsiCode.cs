namespace ThreeXPlusOne.App.Enums;

public enum AnsiCode
{
    [AnsiValue("\x1b[48;2;{0};{1};{2}m")]
    BackgroundColor,

    [AnsiValue("\x1b[D \x1b[D")]
    Backspace,

    [AnsiValue("\x1b[1m")]
    Bold,

    [AnsiValue("\x1b[K")]
    ClearLine,

    [AnsiValue("\x1b[2J\x1b[H")]
    ClearScreen,

    [AnsiValue("\x1b[2m")]
    Dim,

    [AnsiValue("\x1b[38;2;{0};{1};{2}m")]
    ForegroundColor,

    [AnsiValue("\x1b[8m")]
    Hidden,

    [AnsiValue("\x1b[7m")]
    Highlight,

    [AnsiValue("\x1b]8;;{0}\x07{1}\x1b]8;;\x07")]
    Hyperlink,

    [AnsiValue("\x1b[3m")]
    Italic,

    [AnsiValue("\x1b[0m")]
    Reset,

    [AnsiValue("\x1b[9m")]
    Strikethrough,

    [AnsiValue("\x1b[4m")]
    Underline
}

[AttributeUsage(AttributeTargets.Field)]
public class AnsiValueAttribute(string ansiValue) : Attribute
{
    public string AnsiValue { get; } = ansiValue;
}