namespace ThreeXPlusOne.Cli.Models;

public record CommandExecutionSettings()
{
    /// <summary>
    /// Whether or not processing should continue
    /// </summary>
    public bool ContinueExecution { get; set; }

    /// <summary>
    /// A list of all possible command line options for use in help text
    /// </summary>
    /// <remarks>Combined into a list to de-couple the app from any command line libraries
    public List<(string shortName, string longName, string description)> OptionsMetadata { get; set; } = [];

    /// <summary>
    /// Whether or not the user provided a directory path to the settings file
    /// </summary>
    public bool SettingsPathProvided { get; set; }

    /// <summary>
    /// Whether or not the settings file exists at the directory path provided by the user
    /// </summary>
    public bool SettingsPathExists { get; set; }

    /// <summary>
    /// The parsed path to the settings file if provided by the user
    /// </summary>
    public string SettingsPath { get; set; } = "";

    /// <summary>
    /// Whether or not to output help text
    /// </summary>
    public bool WriteHelpText { get; set; }

    /// <summary>
    /// Whether or not to output version text
    /// </summary>
    public bool WriteVersionText { get; set; }

    /// <summary>
    /// Whether or not to output usage text
    /// </summary>
    public bool WriteUsageText { get; set; }
}