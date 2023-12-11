using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class ConsoleOutput
{
    public static void WriteAsciiArtLogo()
    {
        WriteSeparator();
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine(" .d8888b.                              d888   ");
        Console.WriteLine("d88P  Y88b                            d8888   ");
        Console.WriteLine("     .d88P                              888   ");
        Console.WriteLine("    8888\"  888  888        888          888   ");
        Console.WriteLine("     \"Y8b. `Y8bd8P'      8888888        888   ");
        Console.WriteLine("888    888   X88K          888          888   ");
        Console.WriteLine("Y88b  d88P .d8\"\"8b.                     888   ");
        Console.WriteLine(" \"Y8888P\"  888  888                   8888888 ");
        Console.ForegroundColor = ConsoleColor.White;
        WriteSeparator();
    }

    public static void WriteHelpText()
    {
        WriteAsciiArtLogo();

        Console.WriteLine();
        Console.WriteLine("Ensure that a 'settings.json' file exists in the same folder as the executable. It must have the following content:");
        Console.WriteLine("");
        Console.WriteLine("{");

        var lcv = 1;
        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null).ToList();

        Console.ForegroundColor = ConsoleColor.White;

        foreach (var property in settingsProperties)
        {
            var comma = lcv != settingsProperties.Count ? "," : "";

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
        Console.WriteLine($"     {nameof(Settings.UseOnlyTheseNumbers)}: \"\" (comma-separated list of numbers to run the program with. Overrides {nameof(Settings.NumberOfSeries)} and {nameof(Settings.MaxStartingNumber)})");
        Console.WriteLine($"     {nameof(Settings.ExcludeTheseNumbers)}: \"73, 54\" (comma-separated list of numbers not to use)");
        Console.WriteLine($"     {nameof(Settings.GenerateGraph)}: true (whether or not to generate the image of the data)");
        Console.WriteLine($"     {nameof(Settings.GenerateHistogram)}: true (whether or not to generate a histogram of numbers starting from 1-9)");
        Console.WriteLine($"     {nameof(Settings.OutputPath)}: \"C:\\path\\to\\save\\image\\\" (the folder where the image should be placed)");
        Console.ResetColor();
        Console.WriteLine("");

        Console.WriteLine("Note: Increasing settings may cause the program to fail. It depends on the capabilities of the machine running it.");
        Console.WriteLine("");
        Console.WriteLine("");
    }

    public static void WriteSeparator()
    {
        Console.WriteLine();
        Console.WriteLine("----------------------------------------------");
        Console.WriteLine();
    }

    public static void WriteHeading(string headerText)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;

        Console.WriteLine();
        Console.WriteLine(headerText);
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.White;
    }
}