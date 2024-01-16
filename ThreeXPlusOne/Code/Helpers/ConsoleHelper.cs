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

    public void SetForegroundColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }

    public void SetCursorVisibility(bool visible)
    {
        Console.CursorVisible = visible;
    }

    public void SetCursorPosition(int left, int top)
    {
        Console.SetCursorPosition(left, top);
    }

    public void WriteSettings()
    {
        WriteHeading("Settings");

        var settingsFileExists = File.Exists(_settings.SettingsFileName);
        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate).ToList();

        foreach (var property in settingsProperties)
        {
            var value = property.GetValue(_settings, null);

            SetForegroundColor(ConsoleColor.Blue);

            Write($"    {property.Name}: ");

            SetForegroundColor(ConsoleColor.White);

            if ((value?.ToString() ?? "").Length > 100)
            {
                value = TruncateLongSettings(value?.ToString() ?? "");
            }

            Write($"{value}");

            WriteLine("");
        }

        if (_settings.GraphDimensions != _settings.SanitizedGraphDimensions)
        {
            WriteLine($"\nInvalid GraphDimensions ({_settings.GraphDimensions}). Defaulted to {_settings.SanitizedGraphDimensions}.");
        }

        if (!settingsFileExists)
        {
            WriteLine($"\nFile '{_settings.SettingsFileName}' not found. Used defaults.");
        }
    }

    public void WriteSettingsSavedMessage(bool savedSettings)
    {
        if (savedSettings)
        {
            WriteLine($"\nSaved generated numbers to '{_settings.SettingsFileName}'\n");
        }
        else
        {
            WriteLine("\nSettings left unchanged\n");
        }
    }

    public void WriteError(string message)
    {
        SetForegroundColor(ConsoleColor.Red);
        Write("\nERROR: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine($"{message}\n");
    }

    public void WriteDone()
    {
        SetForegroundColor(ConsoleColor.Green);
        WriteLine("Done\n");
        SetForegroundColor(ConsoleColor.White);
    }

    public bool ReadYKeyToProceed(string message)
    {
        Write($"{message} (y/n): ");

        ConsoleKeyInfo keyInfo = Console.ReadKey();
        WriteLine("");

        return keyInfo.Key == ConsoleKey.Y;
    }

    public void WriteSeparator()
    {
        WriteLine("\n------------------------------------------------------------------------------------");
    }

    public void WriteHeading(string headerText)
    {
        WriteSeparator();

        SetForegroundColor(ConsoleColor.DarkYellow);

        WriteLine($"\n{headerText}\n");

        SetForegroundColor(ConsoleColor.White);
    }

    public void WriteHelpText()
    {
        WriteAsciiArtLogo();

        WriteHeading("Credits");
        WriteLine("Inspiration from Veritasium: https://www.youtube.com/watch?v=094y1Z2wpJg");
        WriteLine("ASCII art via: https://www.patorjk.com/software/taag/#p=display");
        WriteLine("Graphs drawn with SkiaSharp: https://github.com/mono/SkiaSharp\n");

        WriteHeading("GitHub repository");
        WriteLine("https://github.com/wdthem/ThreeXPlusOne\n");

        WriteHeading("Usage information");
        WriteLine($"To apply custom settings, ensure that a '{_settings.SettingsFileName}' file exists in the same folder as the executable. It must have the following content:\n");
        WriteLine("{");

        var lcv = 1;
        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate).ToList();

        SetForegroundColor(ConsoleColor.White);

        foreach (var property in settingsProperties)
        {
            var comma = lcv != settingsProperties.Count ? "," : "";

            SetForegroundColor(ConsoleColor.Blue);

            Write($"    {property.Name}: ");

            SetForegroundColor(ConsoleColor.White);

            if (property.PropertyType == typeof(string))
            {
                WriteLine("\"[value]\"");
            }
            else
            {
                WriteLine("[value]");
            }

            lcv++;
        }

        SetForegroundColor(ConsoleColor.White);
        WriteLine("}");

        WriteLine("\nA good starting point for settings:\n");
        SetForegroundColor(ConsoleColor.White);

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.NumberOfSeries)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("200 (the total number of series that will run)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.MaxStartingNumber)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("1000 (the highest number any given series can start with)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.UseTheseNumbers)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine($"\"\" (comma-separated list of numbers to run the program with. Overrides {nameof(Settings.NumberOfSeries)} and {nameof(Settings.MaxStartingNumber)})");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.ExcludeTheseNumbers)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("\"\" (comma-separated list of numbers not to use)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.NodeRotationAngle)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("0 (the size of the rotation angle. 0 is no rotation. When using rotation, start small, such as 0.8)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.NodeRadius)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("50 for 2D, 275 for 3D (the radius of the nodes in pixels)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.IncludePolygonsAsNodes)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("false (whether or not to use circles or polygons + circles as graph nodes)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.XNodeSpacer)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("125 for 2D, 250 for 3D (the space between nodes on the x-axis)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.YNodeSpacer)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("125 for 2D, 225 for 3D (the space between nodes on the y-axis)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.DistanceFromViewer)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("200 (for the 3D graph, the distance from the view when applying the perspective transformation)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.GraphDimensions)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("2 (the number of dimensions to render in the graph - 2 or 3)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.DrawConnections)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("true (whether or not to draw connections between the nodes in the graph - if true can increase image file size substantially)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.DrawNumbersOnNodes)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("true (whether or not to draw the numbers at the center of the node that the node represents)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.GenerateGraph)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("true (whether or not to generate the visualization of the data)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.GenerateHistogram)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("true (whether or not to generate a histogram of the distribution of numbers starting from 1-9)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.GenerateMetadataFile)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("true (whether or not to generate a file with metadata about the run)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.GenerateLightSource)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("false (whether or not to generate a light source from the top left of the graph that interacts with nodes)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.GenerateBackgroundStars)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("false (whether or not to generate random stars in the background of the graph)");

        SetForegroundColor(ConsoleColor.Blue);
        Write($"    {nameof(Settings.OutputPath)}: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine("\"C:\\path\\to\\save\\image\\\" (the folder in which the output files should be placed)\n");

        WriteLine("Note: Increasing some settings may result in large canvas sizes, which could cause the program to fail. It depends on the capabilities of the machine running it.\n\n");
    }

    public void WriteAsciiArtLogo()
    {
        SetForegroundColor(ConsoleColor.Blue);

        //line 1
        Write("\n\n_____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\\\\\\\\\\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________________________________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_        ");

        //line 2
        Write(" ___");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\///////\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("__________________________________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\\\\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_       ");

        //line 3
        Write("  __");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///");
        SetForegroundColor(ConsoleColor.Blue);
        Write("______");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/////\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_      ");

        //line 4
        Write("   _________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\//");
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("___________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_     ");

        //line 5
        Write("    ________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\////\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("__");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///\\\\\\/\\\\\\/");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_____________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\\\\\\\\\\\\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_    ");

        //line 6
        Write("     ___________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\//\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("___");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///\\\\\\/");
        SetForegroundColor(ConsoleColor.Blue);
        Write("______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/////\\\\\\///");
        SetForegroundColor(ConsoleColor.Blue);
        Write("________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_   ");

        //line 7
        Write("      __");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("______");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("___________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_  ");

        //line 8
        Write("       _");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///\\\\\\\\\\\\\\\\\\/");
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\/\\///\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///");
        SetForegroundColor(ConsoleColor.Blue);
        Write("____________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_ ");

        //line 9
        Write("        ___");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/////////");
        SetForegroundColor(ConsoleColor.Blue);
        Write("_____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///");
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///");
        SetForegroundColor(ConsoleColor.Blue);
        Write("________________________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_ ");

        SetForegroundColor(ConsoleColor.White);
    }

    public void WriteProcessEnd(TimeSpan timespan)
    {
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           timespan.Minutes, timespan.Seconds, timespan.Milliseconds);

        WriteHeading($"Process completed");
        WriteLine($"Execution time: {elapsedTime}\n\n");
    }

    public void WriteSpinner(CancellationToken token)
    {
        int counter = 0;
        string[] spinner = ["|", "/", "-", "\\"];

        SetCursorVisibility(false);

        while (!token.IsCancellationRequested)
        {
            Write($"{spinner[counter]}");

            SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

            counter = (counter + 1) % spinner.Length;

            Thread.Sleep(85);
        }

        SetCursorVisibility(true);
    }

    private string TruncateLongSettings(string input, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        string[] numbers = input.Split(',');
        StringBuilder truncated = new();
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
