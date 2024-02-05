namespace ThreeXPlusOne.CommandLine.Models;

public record CommandExecutionSettings()
{
    /// <summary>
    /// The name of the app settings file
    /// </summary>
    private readonly string _appSettingsFileName = "appSettings.json";

    /// <summary>
    /// The name of the app settings file
    /// </summary>
    public string AppSettingsFileName => _appSettingsFileName;

    /// <summary>
    /// Whether or not processing should continue
    /// </summary>
    public bool ContinueExecution { get; set; }

    /// <summary>
    /// A list of all possible command line options for use in help text
    /// </summary>
    /// <remarks>Combined into a list to de-couple the app from any command line libraries
    public List<(string shortName, string longName, string description, string hint)> OptionsMetadata { get; set; } = [];

    /// <summary>
    /// Whether or not the user provided a directory path to the app settings file
    /// </summary>
    public bool AppSettingsPathProvided { get; set; }

    /// <summary>
    /// Whether or not the app settings file exists at the directory path provided by the user
    /// </summary>
    public bool AppSettingsPathExists { get; set; }

    /// <summary>
    /// The full path to the app settings file, which could have been provided by the user
    /// </summary>
    public string AppSettingsFileFullPath { get; set; } = "";

    /// <summary>
    /// Whether or not to output help text
    /// </summary>
    public bool WriteHelpText { get; set; }

    /// <summary>
    /// Whether or not to output version text
    /// </summary>
    public bool WriteVersionText { get; set; }

    /// <summary>
    /// Whether or not to output configuration text
    /// </summary>
    public bool WriteConfigText { get; set; }

    /// <summary>
    /// Messages to surface to the user as a result of parsing the command line commands
    /// </summary>
    public List<string> CommandParsingMessages { get; set; } = [];
}