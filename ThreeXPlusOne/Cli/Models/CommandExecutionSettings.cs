namespace ThreeXPlusOne.Cli.Models;

public record CommandExecutionSettings()
{
    /// <summary>
    /// Whether or not processing should continue
    /// </summary>
    public bool ContinueExecution { get; set; }

    /// <summary>
    /// The parsed path to the settings file if provided by the user
    /// </summary>
    public string? SettingsPath { get; set; }
}