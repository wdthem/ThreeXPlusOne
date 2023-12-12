using System.Text.Json;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Config;


if (args.Length > 0)
{
    if (args[0] == "--help")
    {
        ConsoleOutput.WriteHelpText();

        return;
    }
}

Settings? settings;
string json = File.ReadAllText("settings.json");

try
{
    settings = JsonSerializer.Deserialize<Settings>(json)
        ?? throw new Exception("Setting are null");
}
catch(Exception e)
{
    ConsoleOutput.WriteError($"Could not load settings. Please check 'settings.json'. Error was: {e.Message}");

    return;
}

try
{
    Process.Run(settings);
}
catch(Exception e)
{
    ConsoleOutput.WriteError($"\n{e.Message}");
}