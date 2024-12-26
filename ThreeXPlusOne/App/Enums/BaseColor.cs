namespace ThreeXPlusOne.App.Enums;

public enum BaseColor
{
    [AppColorValue(AppColor.Gray)]
    Foreground,

    [AppColorValue(AppColor.VsCodeGray)]
    Background
}

[AttributeUsage(AttributeTargets.Field)]
public class AppColorValueAttribute(AppColor color) : Attribute
{
    public AppColor AppColor { get; } = color;
}