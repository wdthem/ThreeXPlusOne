namespace ThreeXPlusOne.CommandLine.Models;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class CommandLineHintAttribute(string hint) : Attribute
{
    /// <summary>
    /// A hint for use in help text
    /// </summary>
    public string Hint { get; } = hint;
}