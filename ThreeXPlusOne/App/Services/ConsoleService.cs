using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public partial class ConsoleService(IOptions<AppSettings> appSettings) : IConsoleService
{
    private int _spinnerCounter = 0;
    private static readonly object _consoleLock = new();
    private readonly AppSettings _appSettings = appSettings.Value;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string[] _spinner = ["|", "/", "-", "\\"];
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SplitToWordsRegex();

    /// <summary>
    /// Truncate settings values that are long to avoid carriage returns.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="maxLength"></param>
    /// <returns></returns>
    private string TruncateLongSettings(string input, int maxLength = 100)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        string[] numbers = input.Split(',');
        StringBuilder truncated = new();
        string ellipsis = $" ...see {_appSettings.SettingsFileName} for full value";

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

    /// <summary>
    /// Generate the text for suggested values for the app settings.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="instance"></param>
    /// <param name="sectionName"></param>
    /// <returns></returns>
    private List<(ConsoleColor, string)> GenerateSuggestedValueText(Type? type = null,
                                                                    object? instance = null,
                                                                    string? sectionName = null)
    {
        List<(ConsoleColor, string)> lines = [];
        type ??= typeof(AppSettings);
        instance ??= _appSettings;

        List<PropertyInfo> appSettingsProperties = type.GetProperties()
                                                       .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                                                       .ToList();

        if (!string.IsNullOrWhiteSpace(sectionName))
        {
            SetForegroundColor(ConsoleColor.Blue);

            string sectionNameWords = $"{SplitToWordsRegex().Replace(sectionName, "$1 $2")}:\n";

            lines.Add((ConsoleColor.Blue, sectionNameWords));
        }

        bool generalSettingsWritten = false;

        foreach (PropertyInfo property in appSettingsProperties)
        {
            Type propertyType = property.PropertyType;

            if (propertyType.IsClass && !propertyType.Equals(typeof(string)))
            {
                object? nextInstance = property.GetValue(instance);

                lines.AddRange(GenerateSuggestedValueText(propertyType, nextInstance, property.Name));
            }
            else
            {
                if (type == typeof(AppSettings) && !generalSettingsWritten)
                {
                    lines.Add((ConsoleColor.Blue, "General Settings:\n"));

                    generalSettingsWritten = true;
                }

                lines.Add((ConsoleColor.Blue, $"  {property.Name}"));

                AppSettingAttribute? settingAttribute = property.GetCustomAttribute<AppSettingAttribute>();

                if (settingAttribute != null)
                {
                    lines.Add((ConsoleColor.White, $"  {settingAttribute.Description.Replace("{LightSourcePositionsPlaceholder}", string.Join(", ", Enum.GetNames(typeof(LightSourcePosition)).OrderBy(name => name)))
                                                                                    .Replace("{ImageTypesPlaceholder}", string.Join(", ", Enum.GetNames(typeof(ImageType)).OrderBy(name => name)))
                                                                                    .Replace("{GraphTypePlaceholder}", string.Join(", ", Enum.GetNames(typeof(GraphType)).OrderBy(name => name)))
                                                                                    .Replace("{ShapesPlaceholder}", string.Join(", ", Enum.GetNames(typeof(ShapeType)).OrderBy(name => name)))}"));

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
        }

        return lines;
    }

    /// <summary>
    /// Allow the user to scroll through the settings output, as it is longer than a standard console window's height.
    /// </summary>
    /// <param name="outputType"></param>
    /// <param name="lines"></param>
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

    /// <summary>
    /// Custom write method for console output (threadsafe).
    /// </summary>
    /// <param name="message"></param>
    /// <param name="delay"></param>
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

                Thread.Sleep(Random.Shared.Next(1, 6));
            }
        }
    }

    /// <summary>
    /// Custom WriteLine method for console output (threadsafe).
    /// </summary>
    /// <param name="message"></param>
    public void WriteLine(string message)
    {
        lock (_consoleLock)
        {
            Console.WriteLine(message);
        }
    }

    /// <summary>
    /// Set the foreground colour (threadsafe).
    /// </summary>
    /// <param name="color"></param>
    public void SetForegroundColor(ConsoleColor color)
    {
        lock (_consoleLock)
        {
            Console.ForegroundColor = color;
        }
    }

    /// <summary>
    /// Set cursor visibility (threadsafe).
    /// </summary>
    /// <param name="visible"></param>
    public void SetCursorVisibility(bool visible)
    {
        lock (_consoleLock)
        {
            Console.CursorVisible = visible;
        }
    }

    /// <summary>
    /// Set the cursor position (threadsafe).
    /// </summary>
    /// <param name="left"></param>
    /// <param name="top"></param>
    public void SetCursorPosition(int left, int top)
    {
        lock (_consoleLock)
        {
            Console.SetCursorPosition(left, top);
        }
    }

    /// <summary>
    /// Write the app settings to the screen.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="instance"></param>
    /// <param name="sectionName"></param>
    /// <param name="includeHeader"></param>
    /// <param name="isJson"></param>
    public void WriteSettings(Type? type = null,
                              object? instance = null,
                              string? sectionName = null,
                              bool includeHeader = true,
                              bool isJson = false)
    {
        type ??= typeof(AppSettings);
        instance ??= _appSettings;

        if (includeHeader)
        {
            WriteHeading("Settings");
        }

        if (!string.IsNullOrWhiteSpace(sectionName))
        {
            SetForegroundColor(ConsoleColor.Blue);

            string sectionNameWords = isJson
                                        ? $"\"{sectionName}\":"
                                        : $"{SplitToWordsRegex().Replace(sectionName, "$1 $2")}:";

            if (isJson)
            {
                Write($"    {sectionNameWords}");
                SetForegroundColor(ConsoleColor.DarkYellow);
                WriteLine("{");
            }
            else
            {
                WriteLine($"    {sectionNameWords}");
            }
        }

        List<PropertyInfo> appSettingsProperties = type.GetProperties()
                                                       .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                                                       .ToList();

        int lcv = 1;
        bool generalSettingsWritten = false;

        foreach (PropertyInfo property in appSettingsProperties)
        {
            Type propertyType = property.PropertyType;

            if (propertyType.IsClass && !propertyType.Equals(typeof(string)))
            {
                object? nextInstance = property.GetValue(instance);

                WriteSettings(propertyType, nextInstance, property.Name, false, isJson);
            }
            else
            {
                if (type == typeof(AppSettings) && !isJson && !generalSettingsWritten)
                {
                    SetForegroundColor(ConsoleColor.Blue);
                    WriteLine("    General Settings:");

                    generalSettingsWritten = true;
                }

                object? value = property.GetValue(instance, null);

                SetForegroundColor(ConsoleColor.Blue);

                if (isJson)
                {
                    if (type != typeof(AppSettings))
                    {
                        Write("    ");
                    }

                    Write($"    \"{property.Name}\": ");
                }
                else
                {
                    Write($"        {property.Name}: ");
                }

                SetForegroundColor(ConsoleColor.White);

                if ((value?.ToString() ?? "").Length > 100)
                {
                    value = TruncateLongSettings(value?.ToString() ?? "");
                }

                if (!isJson)
                {
                    Write($"{value}");
                }
                else
                {
                    if (property.PropertyType == typeof(string))
                    {
                        Write("\"[value]\"");
                    }
                    else
                    {
                        Write("[value]");
                    }

                    if (lcv < appSettingsProperties.Count)
                    {
                        Write(",");
                    }
                }

                WriteLine("");
            }

            lcv++;
        }

        if (isJson && type != typeof(AppSettings))
        {
            SetForegroundColor(ConsoleColor.DarkYellow);
            WriteLine("    },");
        }
    }

    /// <summary>
    /// Write messages that came from parsing command line parameters.
    /// </summary>
    /// <param name="commandParsingMessages"></param>
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
            WriteLine($"Saved generated numbers to '{_appSettings.SettingsFileFullPath}'");
        }
        else
        {
            WriteLine("Settings left unchanged");
        }
    }

    /// <summary>
    /// Write an error message.
    /// </summary>
    /// <param name="message"></param>
    public void WriteError(string message)
    {
        SetForegroundColor(ConsoleColor.Red);
        Write("\nERROR: ");
        SetForegroundColor(ConsoleColor.White);
        WriteLine($"{message}\n");
    }

    /// <summary>
    /// Write a message indicating the given step completed.
    /// </summary>
    public void WriteDone()
    {
        SetForegroundColor(ConsoleColor.Green);
        WriteLine("Done");
        SetForegroundColor(ConsoleColor.White);
    }

    /// <summary>
    /// Read the y or n key press by the user to know whether or not to proceed.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool ReadYKeyToProceed(string message)
    {
        Write($"{message} (y/n): ");

        SetForegroundColor(ConsoleColor.Gray);
        ConsoleKeyInfo keyInfo = Console.ReadKey();

        return keyInfo.Key == ConsoleKey.Y;
    }

    /// <summary>
    /// Write a visual separator in console output.
    /// </summary>
    public void WriteSeparator(bool delay = false)
    {
        Write("------------------------------------------------------------------------------------\n", delay);
    }

    /// <summary>
    /// Write a section heading in console output.
    /// </summary>
    /// <param name="headerText"></param>
    public void WriteHeading(string headerText)
    {
        WriteLine("");
        SetForegroundColor(ConsoleColor.Blue);
        WriteSeparator();

        SetForegroundColor(ConsoleColor.DarkYellow);
        WriteLine($"{headerText.ToUpper()}");

        SetForegroundColor(ConsoleColor.Blue);
        WriteSeparator();
        WriteLine("");

        SetForegroundColor(ConsoleColor.White);
    }

    /// <summary>
    /// Write the app's help text to the console.
    /// </summary>
    /// <param name="commandLineOptions"></param>
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

    /// <summary>
    /// Write the app's usage info to the console.
    /// </summary>
    /// <param name="commandLineOptions"></param>
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

    /// <summary>
    /// Write info about the app's config settings to the console.
    /// </summary>
    public void WriteConfigText()
    {
        WriteAsciiArtLogo();
        WriteHeading("Configuration");

        WriteHeading("App settings");
        WriteLine("If no custom app settings are supplied, defaults will be used.\n");
        WriteLine($"To apply custom app settings, place a file called '{_appSettings.SettingsFileName}' in the same folder as the executable. Or use the --settings flag to provide a directory path to the '{_appSettings.SettingsFileName}' file.\n\nIt must have the following content:\n");

        SetForegroundColor(ConsoleColor.DarkYellow);
        WriteLine("{");

        WriteSettings(type: null,
                      instance: null,
                      sectionName: null,
                      includeHeader: false,
                      isJson: true);

        SetForegroundColor(ConsoleColor.DarkYellow);
        WriteLine("}\n");

        WriteHeading("Definitions and suggested values");

        List<(ConsoleColor, string)> lines = GenerateSuggestedValueText();

        lines.Add((ConsoleColor.White, "\nThe above app settings are a good starting point from which to experiment.\n"));
        lines.Add((ConsoleColor.White, "Alternatively, start with the app settings from the Example Output on the GitHub repository: https://github.com/wdthem/ThreeXPlusOne/blob/main/ThreeXPlusOne.ExampleOutput/ExampleOutputSettings.txt\n"));

        ScrollOutput("app settings", lines);

        WriteHeading("Performance");
        WriteLine("Be aware that increasing some app settings may result in large canvas sizes, which could cause the program to fail. It depends on the capabilities of the machine running it.\n\n");
    }

    /// <summary>
    /// Write the app's version information to the console.
    /// </summary>
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

    /// <summary>
    /// Write the app's ASCII art logo to the console.
    /// </summary>
    public void WriteAsciiArtLogo()
    {
        Console.Clear();
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

    /// <summary>
    /// Write a spinning bar to the console in a threadsafe way to indicate an ongoing process.
    /// </summary>
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

    /// <summary>
    /// Stop the spinning bar in a threadsafe way.
    /// </summary>
    public void StopSpinningBar()
    {
        _cancellationTokenSource?.Cancel();

        SetCursorVisibility(true);
    }

    /// <summary>
    /// Write summary info when the process completes.
    /// </summary>
    /// <param name="timespan"></param>
    public void WriteProcessEnd(TimeSpan timespan)
    {
        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           timespan.Minutes, timespan.Seconds, timespan.Milliseconds);

        WriteHeading($"Process completed");
        WriteLine($"Execution time: {elapsedTime}\n\n");
    }
}