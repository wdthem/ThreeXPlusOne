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
                OutputHelpText();

                return;
            }
        }

        string jsonFilePath = "settings.json";
        string json = File.ReadAllText(jsonFilePath);

        Settings? settings = JsonSerializer.Deserialize<Settings>(json)
            ?? throw new Exception("Invalid settings. Ensure 'settings.json' is in the same folder as the executable");

        Process.Run(settings);
    }

    internal static void OutputHelpText()
    {
        Console.WriteLine("");
        Console.WriteLine("3x + 1 Visualizer - Help");
        Console.WriteLine("------------------------");
        Console.WriteLine("");
        Console.WriteLine("Ensure that a 'settings.json' file exists in the same folder as the executable that looks like the following:");
        Console.WriteLine("");
        Console.WriteLine("{");

        var lcv = 1;
        var settingsProperties = typeof(Settings).GetProperties();

        foreach (var property in settingsProperties)
        {
            var comma = lcv != settingsProperties.Length ? "," : "";

            Console.WriteLine($"    \"{property.Name}\": [value]{comma}");

            lcv++;
        }

        Console.WriteLine("}");

        Console.WriteLine("");
        Console.WriteLine("");

        Console.WriteLine("Note: Increasing settings may cause the program to fail. It depends on the capabilities of the machine running it.");
        Console.WriteLine("");
        Console.WriteLine("");
    }
}