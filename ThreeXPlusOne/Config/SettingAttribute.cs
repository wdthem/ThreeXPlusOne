namespace ThreeXPlusOne.Config;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class SettingAttribute(string description, string suggestedValue) : Attribute
{
    /// <summary>
    /// A human-readable description of the setting for use in help text
    /// </summary>
    public string Description { get; } = description;

    /// <summary>
    /// A value to use as a starting point for the given setting
    /// </summary>
    public string SuggestedValue { get; } = suggestedValue;
}