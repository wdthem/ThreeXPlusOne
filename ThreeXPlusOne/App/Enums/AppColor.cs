namespace ThreeXPlusOne.App.Enums;

public enum AppColor
{
    [HexCode("#4EC9B0")]
    AquaTeal,

    [HexCode("#569CD6")]
    BlueTint,

    [HexCode("#F48B8B")]
    BlushRed,

    [HexCode("#1AC578")]
    BrightJade,

    [HexCode("#FFDE00")]
    EmojiYellow,

    [HexCode("#DEDEDE")]
    Gray,

    [HexCode("#9BDBFD")]
    IcyBlue,

    [HexCode("#A0A0A0")]
    MediumGray,

    [HexCode("#FFFF00")]
    PureYellow,

    [HexCode("#BA88B6")]
    SoftOrchid,

    [HexCode("#1E1E1E")]
    VsCodeGray,

    [HexCode("#C48F74")]
    WarmSand,

    [HexCode("#F5F5F5")]
    WhiteSmoke
}

[AttributeUsage(AttributeTargets.Field)]
public class HexCodeAttribute(string hexCode) : Attribute
{
    public string HexCode { get; } = hexCode;
}