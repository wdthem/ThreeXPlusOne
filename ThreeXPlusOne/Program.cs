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
        Console.WriteLine("Ensure that a 'settings.json' file exists in the same folder as the executable. It must have the following content:");
        Console.WriteLine("");
        Console.WriteLine("{");

        var lcv = 1;
        var settingsProperties = typeof(Settings).GetProperties();

        Console.ForegroundColor = ConsoleColor.White;

        foreach (var property in settingsProperties)
        {
            var comma = lcv != settingsProperties.Length ? "," : "";

            Console.WriteLine($"    \"{property.Name}\": [value]{comma}");

            lcv++;
        }

        Console.ResetColor();
        Console.WriteLine("}");

        Console.WriteLine("");
        Console.WriteLine("A useful starting point for settings:");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("");
        Console.WriteLine($"     {nameof(Settings.CanvasWidth)}: 30000 (the width of the drawing canvas)");
        Console.WriteLine($"     {nameof(Settings.CanvasHeight)}: 35000 (the height of the drawing canvas)");
        Console.WriteLine($"     {nameof(Settings.RotationAngle)}: 0 (the size of the rotation angle. 0 is no rotation)");
        Console.WriteLine($"     {nameof(Settings.XNodeSpacer)}: 125 (the space between nodes on the x-axis)");
        Console.WriteLine($"     {nameof(Settings.YNodeSpacer)}: 125 (the space between nodes on the y-axis)");
        Console.WriteLine($"     {nameof(Settings.NumberOfSeries)}: 200 (the total number of series that will run)");
        Console.WriteLine($"     {nameof(Settings.MaxStartingNumber)}: 1000 (the highest number any given series can start with)");
        Console.WriteLine($"     {nameof(Settings.GenerateImage)}: true (whether or not to generate the image of the data)");
        Console.WriteLine($"     {nameof(Settings.ImagePath)}: \"C:\\path\\to\\save\\image\\\" (the folder where the image should be placed)");
        Console.ResetColor();
        Console.WriteLine("");

        Console.WriteLine("Note: Increasing settings may cause the program to fail. It depends on the capabilities of the machine running it.");
        Console.WriteLine("");
        Console.WriteLine("");
    }
}