using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Helpers;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.App.Helpers;

public class ConsoleHelper(IOptions<Settings> settings) : IConsoleHelper
{
    private readonly Settings _settings = settings.Value;
    private static readonly object _consoleLock = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private int _spinnerCounter = 0;
    private readonly string[] _spinner = ["|", "/", "-", "\\"];

    public void Write(string message)
    {
        lock (_consoleLock)
        {
            Console.Write(message);
        }
    }

    public void WriteLine(string message)
    {
        lock (_consoleLock)
        {
            Console.WriteLine(message);
        }
    }

    public void SetForegroundColor(ConsoleColor color)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = color;
        }
    }

    public void SetCursorVisibility(bool visible)
    {
        lock (_consoleLock)
        {
            Console.CursorVisible = visible;
        }
    }

    public void SetCursorPosition(int left, int top)
    {
        lock (_consoleLock)
        {
            Console.SetCursorPosition(left, top);
        }
    }

    public void WriteSettings()
    {
        WriteHeading("Settings");

        bool settingsFileExists = File.Exists(_settings.SettingsFileName);
        List<PropertyInfo> settingsProperties =
            typeof(Settings).GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate).ToList();

        foreach (PropertyInfo property in settingsProperties)
        {
            object? value = property.GetValue(_settings, null);

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
        SetForegroundColor(ConsoleColor.White);

        WriteSeparator();

        SetForegroundColor(ConsoleColor.DarkYellow);

        WriteLine($"\n{headerText}\n");

        SetForegroundColor(ConsoleColor.White);
    }

    public void WriteHelpText(List<(string longName, string shortName, string description)> commandLineOptions)
    {
        WriteAsciiArtLogo();
        WriteHeading("Help");

        WriteHeading("Commands");

        foreach ((string longName, string shortName, string description) in commandLineOptions)
        {
            WriteLine($"  -{shortName}, --{longName}\t\t{description}");
        }

        WriteLine("");

        WriteHeading("Version");
        WriteVersionText();

        WriteHeading("GitHub repository");
        WriteLine("https://github.com/wdthem/ThreeXPlusOne\n");

        WriteHeading("Credits");
        WriteLine("Inspiration from Veritasium: https://www.youtube.com/watch?v=094y1Z2wpJg");
        WriteLine("ASCII art via: https://www.patorjk.com/software/taag/#p=display");
        WriteLine("Graphs drawn with SkiaSharp: https://github.com/mono/SkiaSharp\n\n");
    }

    public void WriteUsageText()
    {
        Type settingsType = typeof(Settings);

        WriteAsciiArtLogo();
        WriteHeading("Usage information");

        WriteHeading("App settings");
        WriteLine("\nIf no custom settings are supplied, app defaults will be used.\n");
        WriteLine($"To apply custom settings, place a file called '{_settings.SettingsFileName}' in the same folder as the executable.\n\nIt must have the following content:\n");
        WriteLine("{");

        int lcv = 1;
        List<PropertyInfo> settingsProperties =
            typeof(Settings).GetProperties().Where(p => p.SetMethod != null && !p.SetMethod.IsPrivate).ToList();

        SetForegroundColor(ConsoleColor.White);

        foreach (PropertyInfo property in settingsProperties)
        {
            string comma = lcv != settingsProperties.Count ? "," : "";

            SetForegroundColor(ConsoleColor.Blue);

            Write($"  {property.Name}: ");

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
        WriteLine("}\n");

        WriteHeading("Suggested values and explanations");
        SetForegroundColor(ConsoleColor.White);

        lcv = 1;
        foreach (PropertyInfo property in settingsProperties)
        {
            string comma = lcv != settingsProperties.Count ? "," : "";

            SetForegroundColor(ConsoleColor.Blue);

            Write($"  {property.Name}:\t");

            if (property.Name.Length < 12)
            {
                Write("\t\t");
            }
            else if (property.Name.Length < 22)
            {
                Write("\t");
            }

            SetForegroundColor(ConsoleColor.White);

            SettingAttribute? attribute = property.GetCustomAttribute<SettingAttribute>();

            if (attribute != null)
            {
                if (property.PropertyType == typeof(string))
                {
                    Write($"\"{attribute.SuggestedValue}\"");
                }
                else
                {
                    Write($"{attribute.SuggestedValue}");
                }

                WriteLine($"\t[{attribute.Description.Replace("{LightSourcePositionsPlaceholder}", string.Join(", ", Enum.GetNames(typeof(LightSourcePosition))))}]");
            }

            lcv++;
        }

        WriteLine("\n\nThe above settings are a good starting point from which to experiment.\n");
        WriteLine("Alternatively, start with the settings from the Example Output on the GitHub repository: https://github.com/wdthem/ThreeXPlusOne/blob/main/ThreeXPlusOne.ExampleOutput/ExampleOutputSettings.txt\n");

        WriteHeading("Performance");
        WriteLine("Be aware that increasing some settings may result in large canvas sizes, which could cause the program to fail. It depends on the capabilities of the machine running it.\n\n");
    }

    public void WriteVersionText()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        AssemblyInformationalVersionAttribute? versionAttribute =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        SetForegroundColor(ConsoleColor.White);

        if (versionAttribute != null)
        {
            string version = versionAttribute.InformationalVersion;
            string[] versionParts = version.Split('+');
            string coreVersion = versionParts[0];

            string? assemblyName = assembly.GetName().Name;

            WriteLine($"{assemblyName}: v{coreVersion}\n");
        }
        else
        {
            WriteLine("Version information not found\n");
        }
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

    public void ShowSpinningBar()
    {
        long previousMilliseconds = 0;
        const long updateInterval = 100;

        _cancellationTokenSource = new CancellationTokenSource();

        Stopwatch stopwatch = Stopwatch.StartNew();

        SetCursorVisibility(false);

        Task.Run(() =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                long currentMilliseconds = stopwatch.ElapsedMilliseconds;

                if (currentMilliseconds - previousMilliseconds >= updateInterval)
                {
                    Write(_spinner[_spinnerCounter]);

                    SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

                    _spinnerCounter = (_spinnerCounter + 1) % _spinner.Length;

                    previousMilliseconds = currentMilliseconds;
                }

                Thread.Yield();
            }

            stopwatch.Stop();
        });
    }

    public void StopSpinningBar()
    {
        _cancellationTokenSource?.Cancel();

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

        foreach (string number in numbers)
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