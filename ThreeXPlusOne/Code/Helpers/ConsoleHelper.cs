using System.Text;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code.Helpers;

public class ConsoleHelper(IOptions<Settings> settings) : IConsoleHelper
{
    private readonly Settings _settings = settings.Value;

    public void Write(string message)
    {
        Console.Write(message);
    }

    public void WriteLine(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteSettings()
    {
        WriteHeading("Settings");

        var settingsFileExists = File.Exists(_settings.SettingsFileName);
        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate).ToList();

        foreach (var property in settingsProperties)
        {
            var value = property.GetValue(_settings, null);

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write($"    {property.Name}: ");

            Console.ForegroundColor = ConsoleColor.White;

            if ((value?.ToString() ?? "").Length > 100)
            {
                value = TruncateLongSettings(value?.ToString() ?? "");
            }

            Console.Write($"{value}");

            Console.WriteLine();
        }

        if (_settings.GraphDimensions != _settings.SanitizedGraphDimensions)
        {
            Console.WriteLine($"\nInvalid GraphDimensions ({_settings.GraphDimensions}). Defaulted to {_settings.SanitizedGraphDimensions}.");
        }

        if (!settingsFileExists)
        {
            Console.WriteLine($"\nFile '{_settings.SettingsFileName}' not found. Used defaults.");
        }
    }

    public void WriteSettingsSavedMessage(bool savedSettings)
    {
        if (savedSettings)
        {
            Console.WriteLine($"Saved generated numbers to '{_settings.SettingsFileName}'\n");
        }
        else
        {
            Console.WriteLine("Settings left unchanged\n");
        }
    }

    public void WriteError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("\nERROR: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"{message}\n");
    }

    public void WriteDone()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Done\n");
        Console.ForegroundColor = ConsoleColor.White;
    }

    public bool ReadYKeyToProceed(string message)
    {
        Console.Write($"{message} (y/n): ");

        ConsoleKeyInfo keyInfo = Console.ReadKey();
        Console.WriteLine();

        return keyInfo.Key == ConsoleKey.Y;
    }

    public void WriteSeparator()
    {
        Console.WriteLine("\n------------------------------------------------------------------------------------");
    }

    public void WriteHeading(string headerText)
    {
        WriteSeparator();

        Console.ForegroundColor = ConsoleColor.DarkYellow;

        Console.WriteLine($"\n{headerText}\n");

        Console.ForegroundColor = ConsoleColor.White;
    }

    public void WriteHelpText()
    {
        WriteAsciiArtLogo();

        WriteHeading("GitHub repository");
        Console.WriteLine("\nhttps://github.com/wdthem/ThreeXPlusOne\n");

        WriteHeading("Usage information");
        Console.WriteLine($"\nTo apply custom settings, ensure that a '{_settings.SettingsFileName}' file exists in the same folder as the executable. It must have the following content:\n");
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

        Console.WriteLine("\nA useful starting point for settings:\n");
        Console.ForegroundColor = ConsoleColor.White;

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.CanvasWidth)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("30000 (the width of the drawing canvas in pixels)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.CanvasHeight)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("35000 (the height of the drawing canvas in pixels)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.NumberOfSeries)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("200 (the total number of series that will run)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.MaxStartingNumber)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("1000 (the highest number any given series can start with)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.UseTheseNumbers)}: ");
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
        Console.Write($"     {nameof(Settings.DistanceFromViewer)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("200 (for the 3D graph, the distance from the view when applying the perspective transformation)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.GraphDimensions)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("2 (the number of dimensions to render in the graph - 2 or 3)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.DrawConnections)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("true (whether or not to draw connections between the nodes in the graph - if true can increase image file size substantially)");

        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"     {nameof(Settings.DrawNumbersOnNodes)}: ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("true (whether or not to draw the numbers at the center of the node that the node represents)");

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
        Console.WriteLine("\"C:\\path\\to\\save\\image\\\" (the folder where the output files should be placed)\n");

        Console.WriteLine("Note: Increasing settings may cause the program to fail. It depends on the capabilities of the machine running it.\n\n");
    }

    public void WriteAsciiArtLogo()
    {
        Console.ForegroundColor = ConsoleColor.Blue;

        //line 1
        Console.Write("\n\n_____");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\\\\\\\\\\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_______________________________________________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_        ");

        //line 2
        Console.Write(" ___");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\///////\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("__________________________________________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\\\\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_       ");

        //line 3
        Console.Write("  __");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("______");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_______________________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_______________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/////\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_      ");

        //line 4
        Console.Write("   _________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\//");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("____");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("____");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_______________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("___________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_     ");

        //line 5
        Console.Write("    ________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\////\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("__");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///\\\\\\/\\\\\\/");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_____________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\\\\\\\\\\\\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_______________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_    ");

        //line 6
        Console.Write("     ___________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\//\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("___");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///\\\\\\/");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("______________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/////\\\\\\///");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_   ");

        //line 7
        Console.Write("      __");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("______");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_____");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("___________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_  ");

        //line 8
        Console.Write("       _");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///\\\\\\\\\\\\\\\\\\/");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("____");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("/\\\\\\/\\///\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_______________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("____________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/\\\\\\");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_ ");

        //line 9
        Console.Write("        ___");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\/////////");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("_____");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("____");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("________________________________________");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("\\///");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("_ ");

        Console.ForegroundColor = ConsoleColor.White;
    }

    public void WriteSpinner(CancellationToken token)
    {
        var spinner = new string[] { "|", "/", "-", "\\" };
        int counter = 0;

        Console.CursorVisible = false;

        while (!token.IsCancellationRequested)
        {
            Console.Write($"{spinner[counter]}");

            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

            counter = (counter + 1) % spinner.Length;

            Thread.Sleep(85);
        }

        WriteDone();
        Console.CursorVisible = true;
    }

    private string TruncateLongSettings(string input, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        string[] numbers = input.Split(',');
        var truncated = new StringBuilder();
        string ellipsis = $" ...see {_settings.SettingsFileName} for full value";

        int lengthWithEllipsis = maxLength - ellipsis.Length;

        foreach (var number in numbers)
        {
            // Check if adding this number exceeds the maximum length
            // +1 for the comma, except for the first number
            if (truncated.Length + number.Length + (truncated.Length > 0 ? 1 : 0) > lengthWithEllipsis)
            {
                truncated.Append(ellipsis);

                break;
            }

            if (truncated.Length > 0)
            {
                truncated.Append(',');
            }

            truncated.Append(number);
        }

        return truncated.ToString();
    }
}