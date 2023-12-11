using System.Text.Json;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne;

public static class Program
{
    public static void Main(string[] args)
    {
        if (args.Length > 0)
        {
            if (args[0] == "--help")
            {
                ConsoleOutput.WriteHelpText();

                return;
            }
        }

        string jsonFilePath = "settings.json";
        string json = File.ReadAllText(jsonFilePath);
        Settings? settings;

        try
        {
            settings = JsonSerializer.Deserialize<Settings>(json)
                ?? throw new Exception();
        }
        catch
        {
            ConsoleOutput.WriteError("Could not load settings. Please check 'settings.json'");

            return;
        }

        try
        {
            Process.Run(settings);
        }
        catch(Exception e)
        {
            ConsoleOutput.WriteError(e.Message);
        }
    }
}