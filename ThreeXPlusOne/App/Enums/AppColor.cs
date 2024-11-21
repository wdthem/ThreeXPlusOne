namespace ThreeXPlusOne.App.Enums;

public enum AppColor
{
    [HexColor("#569CD6")]
    BlueTint,

    [HexColor("#F48B8B")]
    BlushRed,

    [HexColor("#1AC578")]
    BrightJade,

    [HexColor("#4EC9B0")]
    AquaTeal,

    [HexColor("#DEDEDE")]
    Gray,

    [HexColor("#9BDBFD")]
    IcyBlue,

    [HexColor("#A0A0A0")]
    MediumGray,

    [HexColor("#FFFF00")]
    PureYellow,

    [HexColor("#BA88B6")]
    SoftOrchid,

    [HexColor("#1E1E1E")]
    VsCodeGray,

    [HexColor("#C48F74")]
    WarmSand,

    [HexColor("#F5F5F5")]
    WhiteSmoke
}

[AttributeUsage(AttributeTargets.Field)]
public class HexColorAttribute(string hexValue) : Attribute
{
    public string HexValue { get; } = hexValue;
}