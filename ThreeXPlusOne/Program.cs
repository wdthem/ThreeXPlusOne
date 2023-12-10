using System.Text.Json;
using ThreeXPlusOne.Code;
using ThreeXPlusOne.Config;

internal class Program
{
    internal static void Main(string[] args)
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

        Settings? settings = JsonSerializer.Deserialize<Settings>(json)
            ?? throw new Exception("Invalid settings. Ensure 'settings.json' is in the same folder as the executable");

        Process.Run(settings);
    }
}