using ThreeXPlusOne.CommandLine.Models;

namespace ThreeXPlusOne.CommandLine.Services;

public class CommandExecutionSettingsService
{
    public CommandExecutionSettings Settings { get; private set; } = null!;

    /// <summary>
    /// Initialize the settings.
    /// </summary>
    /// <param name="settings"></param>
    public void Initialize(CommandExecutionSettings settings)
    {
        Settings = settings;
    }
}