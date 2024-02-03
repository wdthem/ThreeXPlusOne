using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public class ConsoleService(IOptions<Settings> settings) : IConsoleService
{
    private int _spinnerCounter = 0;
    private static readonly object _consoleLock = new();
    private readonly Settings _settings = settings.Value;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string[] _spinner = ["|", "/", "-", "\\"];
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

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

    private void ScrollOutput(string outputType, List<(ConsoleColor Color, string Text)> lines)
    {
        int currentLine = 0;

        WriteLine($"Press a key to scroll {outputType} text, 'q' to output all...");

        int startLeft = Console.CursorLeft;
        int startTop = Console.CursorTop;

        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(intercept: true).Key;

                if (currentLine == 0)
                {
                    SetCursorPosition(startLeft, startTop - 1);
                    Write(new string(' ', Console.WindowWidth)); // Overwrite the line with spaces
                    SetCursorPosition(startLeft, startTop - 1);
                }

                if (key == ConsoleKey.Q)
                {
                    //output all remaining lines in one go
                    for (int lcv = currentLine; lcv < lines.Count; lcv++)
                    {
                        SetForegroundColor(lines[lcv].Color);
                        WriteLine(lines[lcv].Text);
                    }

                    break;
                }

                if (currentLine < lines.Count)
                {
                    SetForegroundColor(lines[currentLine].Color);
                    WriteLine(lines[currentLine].Text);

                    currentLine++;
                }
                else
                {
                    break;
                }
            }

            Thread.Sleep(100);
        }
    }

    public void Write(string message, bool delay = false)
    {
        lock (_consoleLock)
        {
            if (!delay)
            {
                Console.Write(message);

                return;
            }

            foreach (char character in message.ToCharArray())
            {
                Console.Write(character.ToString());

                Thread.Sleep(Random.Shared.Next(1, 8));
            }
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

        List<PropertyInfo> settingsProperties = [.. typeof(Settings).GetProperties()];

        foreach (PropertyInfo property in settingsProperties)
        {
            JsonIgnoreAttribute? attribute = property.GetCustomAttribute<JsonIgnoreAttribute>();

            if (attribute != null)
            {
                continue;
            }

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
    }

    public void WriteCommandParsingMessages(List<string> commandParsingMessages)
    {
        if (commandParsingMessages.Count == 0)
        {
            return;
        }

        WriteHeading("Information");

        foreach (string message in commandParsingMessages)
        {
            WriteLine(message);
        }

        WriteLine("");
    }

    public void WriteSettingsSavedMessage(bool savedSettings)
    {
        if (savedSettings)
        {
            WriteLine($"\nSaved generated numbers to '{_settings.SettingsFileFullPath}'\n");
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
        Write("\n------------------------------------------------------------------------------------\n", true);
    }

    public void WriteHeading(string headerText)
    {
        SetForegroundColor(ConsoleColor.White);

        WriteSeparator();

        SetForegroundColor(ConsoleColor.DarkYellow);

        WriteLine($"\n{headerText}\n");

        SetForegroundColor(ConsoleColor.White);
    }

    public void WriteHelpText(List<(string longName, string shortName, string description, string hint)> commandLineOptions)
    {
        WriteAsciiArtLogo();
        WriteHeading("Help");

        WriteHeading("Commands");

        WriteCommandUsage(commandLineOptions);

        WriteHeading("Version");
        WriteVersionText();

        WriteHeading("GitHub repository");
        WriteLine("https://github.com/wdthem/ThreeXPlusOne\n");

        WriteHeading("Credits");
        WriteLine("Inspiration from Veritasium: https://www.youtube.com/watch?v=094y1Z2wpJg");
        WriteLine("ASCII art via: https://www.patorjk.com/software/taag/#p=display");
        WriteLine("Graphs drawn with SkiaSharp: https://github.com/mono/SkiaSharp\n\n");
    }

    public void WriteCommandUsage(List<(string longName, string shortName, string description, string hint)> commandLineOptions)
    {
        string? assemblyName = _assembly.GetName().Name;

        Write($"usage: {assemblyName} ");

        int lcv = 1;
        foreach ((string shortName, string longName, string description, string hint) in commandLineOptions)
        {
            if (lcv % 3 == 0)
            {
                Write("\n                     ");
            }

            string hintText = !string.IsNullOrWhiteSpace(hint) ? $" {hint}" : "";

            Write($"[-{shortName} | --{longName}{hintText}] ");

            lcv++;
        }

        WriteLine("\n");

        foreach ((string shortName, string longName, string description, string hint) in commandLineOptions)
        {
            string commandText = $"  -{shortName}, --{longName}";
            Write(commandText);

            if (commandText.Length <= 15)
            {
                Write("\t\t");
            }
            else
            {
                Write("\t");
            }

            WriteLine($"{description}");
        }

        WriteLine("");
    }

    public void WriteConfigText()
    {
        WriteAsciiArtLogo();
        WriteHeading("Configuration");

        WriteHeading("App settings");
        WriteLine("If no custom settings are supplied, app defaults will be used.\n");
        WriteLine($"To apply custom settings, place a file called '{_settings.SettingsFileName}' in the same folder as the executable. Or use the --settings flag to provide a directory path to the '{_settings.SettingsFileName}' file.\n\nIt must have the following content:\n");
        WriteLine("{");

        int lcv = 1;
        List<PropertyInfo> settingsProperties = [.. typeof(Settings).GetProperties()];
        JsonIgnoreAttribute? jsonAttribute;

        SetForegroundColor(ConsoleColor.White);

        foreach (PropertyInfo property in settingsProperties)
        {
            jsonAttribute = property.GetCustomAttribute<JsonIgnoreAttribute>();

            if (jsonAttribute != null)
            {
                continue;
            }

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

        WriteHeading("Definitions and suggested values");

        List<(ConsoleColor, string)> lines = [];

        foreach (PropertyInfo property in settingsProperties)
        {
            jsonAttribute = property.GetCustomAttribute<JsonIgnoreAttribute>();

            if (jsonAttribute != null)
            {
                continue;
            }

            lines.Add((ConsoleColor.Blue, $"  {property.Name}"));

            SettingAttribute? settingAttribute = property.GetCustomAttribute<SettingAttribute>();

            if (settingAttribute != null)
            {
                lines.Add((ConsoleColor.White, $"  {settingAttribute.Description.Replace("{LightSourcePositionsPlaceholder}", string.Join(", ", Enum.GetNames(typeof(LightSourcePosition))))}"));

                string suggestedValueText = "  Suggested value: ";

                if (property.PropertyType == typeof(string))
                {
                    suggestedValueText += $"\"{settingAttribute.SuggestedValue}\"\n";
                }
                else
                {
                    suggestedValueText += $"{settingAttribute.SuggestedValue}\n";
                }

                lines.Add((ConsoleColor.White, suggestedValueText));
            }
        }

        lines.Add((ConsoleColor.White, "\nThe above settings are a good starting point from which to experiment.\n"));
        lines.Add((ConsoleColor.White, "Alternatively, start with the settings from the Example Output on the GitHub repository: https://github.com/wdthem/ThreeXPlusOne/blob/main/ThreeXPlusOne.ExampleOutput/ExampleOutputSettings.txt\n"));

        ScrollOutput("settings", lines);

        WriteHeading("Performance");
        WriteLine("Be aware that increasing some settings may result in large canvas sizes, which could cause the program to fail. It depends on the capabilities of the machine running it.\n\n");
    }

    public void WriteVersionText()
    {
        AssemblyInformationalVersionAttribute? versionAttribute =
            _assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();

        SetForegroundColor(ConsoleColor.White);

        if (versionAttribute != null)
        {
            string version = versionAttribute.InformationalVersion;
            string[] versionParts = version.Split('+');
            string coreVersion = versionParts[0];

            string? assemblyName = _assembly.GetName().Name;

            WriteLine($"{assemblyName}: v{coreVersion}\n");
        }
        else
        {
            WriteLine("Version information not found.\n");
        }
    }

    public void WriteAsciiArtLogo()
    {
        SetForegroundColor(ConsoleColor.Blue);

        //line 1
        Write("\n\n_____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\\\\\\\\\\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________________________________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_        ");

        //line 2
        Write(" ___");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\///////\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("__________________________________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\\\\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_       ");

        //line 3
        Write("  __");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("______");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/////\\\\\\");
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_      ");

        //line 4
        Write("   _________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\//", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("___________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_     ");

        //line 5
        Write("    ________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\////\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("__");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///\\\\\\/\\\\\\/", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_____________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\\\\\\\\\\\\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_    ");

        //line 6
        Write("     ___________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\//\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("___");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///\\\\\\/", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/////\\\\\\///", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_   ");

        //line 7
        Write("      __");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("______");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("___________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_  ");

        //line 8
        Write("       _");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///\\\\\\\\\\\\\\\\\\/", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("/\\\\\\/\\///\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_______________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("____________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/\\\\\\", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_ ");

        //line 9
        Write("        ___");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\/////////", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("_____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("____");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///", true);
        SetForegroundColor(ConsoleColor.Blue);
        Write("________________________________________");
        SetForegroundColor(ConsoleColor.DarkYellow);
        Write("\\///", true);
        SetForegroundColor(ConsoleColor.Blue);
        WriteLine("_ ");

        Write("                                                                                    ", true);
        WriteLine("");
        SetForegroundColor(ConsoleColor.White);
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

    public void WriteProcessEnd(TimeSpan timespan)
    {
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           timespan.Minutes, timespan.Seconds, timespan.Milliseconds);

        WriteHeading($"Process completed");
        WriteLine($"Execution time: {elapsedTime}\n\n");
    }
}