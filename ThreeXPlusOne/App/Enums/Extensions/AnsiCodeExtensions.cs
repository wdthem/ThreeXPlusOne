namespace ThreeXPlusOne.App.Enums.Extensions;

public static class AnsiCodeExtensions
{
    /// <summary>
    /// Get the ANSI code for the given <see cref="AnsiCode"/>.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static string GetCode(this AnsiCode code, params object[] args)
    {
        var attribute = (AnsiValueAttribute?)Attribute.GetCustomAttribute(
            code.GetType().GetField(code.ToString())!,
            typeof(AnsiValueAttribute))
                ?? throw new InvalidOperationException($"No ANSI code found for {code}");

        return string.Format(attribute.AnsiValue, args);
    }
}