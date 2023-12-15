using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class ConsoleOutput
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

    public static void WriteSettings(Settings settings)
    {
        WriteHeading("Settings");

        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate).ToList();

        foreach (var property in settingsProperties)
        {
            var value = property.GetValue(settings, null);

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write($"    {property.Name}: ");

            Console.ForegroundColor = ConsoleColor.White;

            Console.Write($"{value}");
            Console.WriteLine();
        }

        if (settings.GraphDimensions != settings.ParsedGraphDimensions)
        {
            Console.WriteLine();
            Console.WriteLine($"Invalid GraphDimensions ({settings.GraphDimensions}). Defaulted to {settings.ParsedGraphDimensions}.");
        }

        WriteSeparator();
    }

    public static void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine();
        Console.Write("ERROR: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(message);
        Console.WriteLine();
    }

    public static void WriteDone()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Done");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
    }

    public static void WriteHelpText()
    {
        WriteAsciiArtLogo();

        WriteHeading("Usage information");

        Console.WriteLine();
        Console.WriteLine("Ensure that a 'settings.json' file exists in the same folder as the executable. It must have the following content:");
        Console.WriteLine("");
        Console.WriteLine("{");

        var lcv = 1;
        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate).ToList();

        Console.ForegroundColor = ConsoleColor.White;

        foreach (var property in settingsProperties)
        {
            var comma = lcv != settingsProperties.Count ? "," : "";

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write($"    {property.Name}: ");

            Console.ForegroundColor = ConsoleColor.White;

            if (property.PropertyType == typeof(string))
            {
                Console.WriteLine("\"[value]\"");
            }
            else
            {
                Console.WriteLine("[value]");
            }
            
            lcv++;
        }

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("}");

        Console.WriteLine("");
        Console.WriteLine("A useful starting point for settings:");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.CanvasWidth)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("20000 (the width of the drawing canvas in pixels)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.CanvasHeight)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("20000 (the height of the drawing canvas in pixels)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.NumberOfSeries)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("200 (the total number of series that will run)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.MaxStartingNumber)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("1000 (the highest number any given series can start with)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.UseOnlyTheseNumbers)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"\"\" (comma-separated list of numbers to run the program with. Overrides {nameof(Settings.NumberOfSeries)} and {nameof(Settings.MaxStartingNumber)})");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.ExcludeTheseNumbers)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\"\" (comma-separated list of numbers not to use)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.NodeRotationAngle)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("0 (the size of the rotation angle. 0 is no rotation)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.NodeRadius)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("40 for 2D, 300 for 3D (the radius of the nodes in pixels)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.DistortNodes)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("false (whether or not to use circles or distorted shapes as graph nodes)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.RadiusDistortion)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("30 (the max amount by which to distort node radii in pixels)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.XNodeSpacer)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("125 for 2D, 250 for 3D (the space between nodes on the x-axis)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.YNodeSpacer)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("125 (the space between nodes on the y-axis)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.GraphDimensions)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("2 (the number of dimensions to render in the graph - 2 or 3)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.GenerateGraph)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("true (whether or not to generate the visualization of the data)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.GenerateHistogram)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("true (whether or not to generate a histogram of the distribution of numbers starting from 1-9)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.GenerateMetadataFile)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("true (whether or not to generate a file with metadata about the run)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.GenerateBackgroundStars)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("true (whether or not to generate random stars in the background of the graph)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.OutputPath)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\"C:\\path\\to\\save\\image\\\" (the folder where the output files should be placed)");
        Console.WriteLine("");

        Console.WriteLine("Note: Increasing settings may cause the program to fail. It depends on the capabilities of the machine running it.");
        Console.WriteLine();
        Console.WriteLine();
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