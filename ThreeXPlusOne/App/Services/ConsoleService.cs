using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public partial class ConsoleService : IConsoleService
{
    private int _spinnerCounter = 0;
    private static readonly object _consoleLock = new();
    private readonly AppSettings _appSettings;
    private CancellationTokenSource? _cancellationTokenSource;
    private readonly string[] _spinner = ["|", "/", "-", "\\"];
    private readonly string _defaultAnsiForegroundColour;
    private readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SplitToWordsRegex();

    [GeneratedRegex(@"(</?[^>]+>)|([^<>]+)")]
    private static partial Regex ColorMarkup();

    public ConsoleService(IOptions<AppSettings> appSettings)
    {
        _appSettings = appSettings.Value;

        EnableAnsiSupport();
        _defaultAnsiForegroundColour = HexColorToAnsiString(GetHexColor(AppColor.Gray.ToString()));
    }

    /// <summary>
    /// Clear the console screen and set the background color to VsCodeGray.
    /// </summary>
    private void Clear()
    {
        string hexColor = GetHexColor(AppColor.VsCodeGray.ToString());
        Color color = ColorTranslator.FromHtml(hexColor);

        Write($"\x1b[48;2;{color.R};{color.G};{color.B}m");

        // Clear the entire screen with the new background color
        Write("\x1b[2J\x1b[H");

        // set default foreground colour
        Write(_defaultAnsiForegroundColour);
    }

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
    private List<string> GenerateSuggestedValueText(Type? type = null,
                                                    object? instance = null,
                                                    string? sectionName = null)
    {
        List<string> lines = [];
        type ??= typeof(AppSettings);
        instance ??= _appSettings;

        List<PropertyInfo> appSettingsProperties = type.GetProperties()
                                                       .Where(p => p.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                                                       .ToList();

        if (!string.IsNullOrWhiteSpace(sectionName))
        {


            string sectionNameWords = $"{SplitToWordsRegex().Replace(sectionName, "$1 $2")}\n";

            lines.Add($"<AquaTeal>{sectionNameWords}</>");
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
                    lines.Add($"<AquaTeal>General Settings</>\n");

                    generalSettingsWritten = true;
                }

                lines.Add($"  <IcyBlue>{property.Name}</>");

                AppSettingAttribute? settingAttribute = property.GetCustomAttribute<AppSettingAttribute>();

                if (settingAttribute != null)
                {
                    lines.Add($"  {settingAttribute.Description.Replace("{LightSourcePositionsPlaceholder}", string.Join(", ", Enum.GetNames(typeof(LightSourcePosition)).OrderBy(name => name)))
                                                               .Replace("{ImageTypesPlaceholder}", string.Join(", ", Enum.GetNames(typeof(ImageType)).OrderBy(name => name)))
                                                               .Replace("{GraphTypePlaceholder}", string.Join(", ", Enum.GetNames(typeof(GraphType)).OrderBy(name => name)))
                                                               .Replace("{ShapesPlaceholder}", string.Join(", ", Enum.GetNames(typeof(ShapeType)).OrderBy(name => name)))}");

                    string suggestedValueText = "  <BlueTint>Suggested value:</> ";

                    if (property.PropertyType == typeof(string))
                    {
                        suggestedValueText += $"<WarmSand>\"{settingAttribute.SuggestedValue}\"</>\n";
                    }
                    else
                    {
                        suggestedValueText += $"<WarmSand>{settingAttribute.SuggestedValue}</>\n";
                    }

                    lines.Add(suggestedValueText);
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
    private void ScrollOutput(string outputType, List<string> lines)
    {
        int currentLine = 0;

        WriteLine($"\nPress a key to scroll {outputType} text, 'q' to output all...");

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
                        WriteLineWithColorMarkup(lines[lcv]);
                    }

                    break;
                }

                if (currentLine < lines.Count)
                {
                    WriteLineWithColorMarkup(lines[currentLine]);

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
    /// Get the hex color code for an AppColor name.
    /// </summary>
    /// <param name="colorName"></param>
    /// <returns></returns>
    private string GetHexColor(string colorName)
    {
        if (!Enum.TryParse<AppColor>(colorName, true, out var color))
        {
            return colorName;
        }

        FieldInfo? field = typeof(AppColor).GetField(color.ToString());
        var attribute = field?.GetCustomAttribute<HexColorAttribute>();

        return attribute?.HexValue ?? _defaultAnsiForegroundColour;
    }

    /// <summary>
    /// Convert a hex color code to an ANSI escape sequence.
    /// </summary>
    /// <param name="hexColor"></param>
    /// <returns></returns>
    private static string HexColorToAnsiString(string hexColor)
    {
        Color color = ColorTranslator.FromHtml(hexColor);

        return $"\x1b[1m\x1b[38;2;{color.R};{color.G};{color.B}m";
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
    /// Write a message to the console with color markup via ANSI escape codes for full color support.
    /// </summary>
    /// <param name="message"></param>
    public void WriteWithColorMarkup(string message)
    {
        Write(_defaultAnsiForegroundColour);

        foreach (Match match in ColorMarkup().Matches(message))
        {
            if (!match.Value.StartsWith('<'))
            {
                Write(match.Value);
                continue;
            }

            Write(match.Value.StartsWith("</")
                ? _defaultAnsiForegroundColour
                : HexColorToAnsiString(GetHexColor(match.Value.Trim('<', '>'))));
        }

        Write(_defaultAnsiForegroundColour);
    }

    /// <summary>
    /// Write an API action message to the console with color markup as a single line.
    /// </summary>
    /// <param name="message"></param>
    public void WriteLineWithColorMarkup(string message)
    {
        WriteWithColorMarkup(message);
        WriteLine("");
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
            string sectionNameWords = isJson
                                        ? $"\"{sectionName}\":"
                                        : $"{SplitToWordsRegex().Replace(sectionName, "$1 $2")}:";

            if (isJson)
            {
                WriteWithColorMarkup($"    <AquaTeal>{sectionNameWords}</>");
                WriteLineWithColorMarkup("<BlushRed>{</>");
            }
            else
            {
                WriteLineWithColorMarkup($"    <AquaTeal>{sectionNameWords}</>");
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
                    WriteLineWithColorMarkup("    <BlueTint>General Settings:</>");

                    generalSettingsWritten = true;
                }

                object? value = property.GetValue(instance, null);

                if (isJson)
                {
                    if (type != typeof(AppSettings))
                    {
                        Write("    ");
                    }

                    WriteWithColorMarkup($"    <IcyBlue>\"{property.Name}\":</> ");
                }
                else
                {
                    WriteWithColorMarkup($"        <IcyBlue>{property.Name}:</> ");
                }

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
            WriteLineWithColorMarkup("    <BlushRed>},</>");
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
        WriteWithColorMarkup($"\n<BlushRed>Error:</> {message}\n");
    }

    /// <summary>
    /// Write a message indicating the given step completed.
    /// </summary>
    public void WriteDone()
    {
        WriteLineWithColorMarkup("<BrightJade>Done</>");
    }

    /// <summary>
    /// Read the y or n key press by the user to know whether or not to proceed.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool AskForConfirmation(string message)
    {
        WriteWithColorMarkup($"<PureYellow>{message}</> [<WhiteSmoke>y/n</>] ");

        while (true)
        {
            ConsoleKey key = Console.ReadKey(intercept: true).Key;
            string response = key == ConsoleKey.Y ? "y" : "n";
            WriteLine(response);
            return key == ConsoleKey.Y;
        }
    }

    /// <summary>
    /// Write a visual separator in console output.
    /// </summary>
    public void WriteSeparator(bool delay = false)
    {
        WriteWithColorMarkup("\n<MediumGray>————————————————————————————————————————————————————————————————————————————————————</>\n");
    }

    /// <summary>
    /// Write a section heading in console output.
    /// </summary>
    /// <param name="headerText"></param>
    public void WriteHeading(string headerText)
    {
        WriteSeparator();

        WriteWithColorMarkup($"<BlueTint>{headerText.ToUpper()}</>");

        WriteSeparator();
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

        WriteWithColorMarkup("usage: ");
        WriteWithColorMarkup($"<BrightJade>{assemblyName}</> ");

        int lcv = 1;
        foreach ((string shortName, string longName, string description, string hint) in commandLineOptions)
        {
            if (lcv % 3 == 0)
            {
                Write("\n                     ");
            }

            string hintText = !string.IsNullOrWhiteSpace(hint) ? $" {hint}" : "";

            WriteWithColorMarkup($"[<IcyBlue>-{shortName}</> | <IcyBlue>--{longName}</>] ");

            lcv++;
        }

        WriteLine("\n");

        foreach ((string shortName, string longName, string description, string hint) in commandLineOptions)
        {
            string commandText = $"  -{shortName}, --{longName}";


            WriteWithColorMarkup($"  <IcyBlue>-{shortName}</>, <IcyBlue>--{longName}</>");

            if (commandText.Length <= 15)
            {
                Write("\t\t");
            }
            else
            {
                Write("\t");
            }

            WriteLineWithColorMarkup($"{description}");
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

        WriteLineWithColorMarkup("<BlushRed>{</>");

        WriteSettings(type: null,
                      instance: null,
                      sectionName: null,
                      includeHeader: false,
                      isJson: true);

        WriteLineWithColorMarkup("<BlushRed>}</>");

        WriteHeading("Definitions and suggested values");

        List<string> lines = GenerateSuggestedValueText();

        lines.Add("\nThe above app settings are a good starting point from which to experiment.\n");
        lines.Add("Alternatively, start with the app settings from the Example Output on the GitHub repository: https://github.com/wdthem/ThreeXPlusOne/blob/main/ThreeXPlusOne.ExampleOutput/ExampleOutputSettings.txt\n");

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

        if (versionAttribute != null)
        {
            string version = versionAttribute.InformationalVersion;
            string[] versionParts = version.Split('+');
            string coreVersion = versionParts[0];

            string? assemblyName = _assembly.GetName().Name;

            WriteLineWithColorMarkup($"<BrightJade>{assemblyName}</>: v{coreVersion}\n");
        }
        else
        {
            WriteLineWithColorMarkup("Version information not found.\n");
        }
    }

    /// <summary>
    /// Write the app's ASCII art logo to the console.
    /// </summary>
    public void WriteAsciiArtLogo()
    {
        Clear();

        Write("\n\n");

        WriteLineWithColorMarkup("<SoftOrchid>_____</><IcyBlue>/\\\\\\\\\\\\\\\\\\\\</><SoftOrchid>_______________________________________________________</><IcyBlue>/\\\\\\</><SoftOrchid>_</>        ");
        WriteLineWithColorMarkup(" <SoftOrchid>___</><IcyBlue>/\\\\\\///////\\\\\\</><SoftOrchid>__________________________________________________</><IcyBlue>/\\\\\\\\\\\\\\</><SoftOrchid>_       ");
        WriteLineWithColorMarkup("  <SoftOrchid>__</><IcyBlue>\\///</><SoftOrchid>______</><IcyBlue>/\\\\\\</><SoftOrchid>_______________________________</><IcyBlue>/\\\\\\</><SoftOrchid>_______________</><IcyBlue>\\/////\\\\\\</><SoftOrchid>_</>      ");
        WriteLineWithColorMarkup("   <SoftOrchid>_________</><IcyBlue>/\\\\\\//</><SoftOrchid>____</><IcyBlue>/\\\\\\</><SoftOrchid>____</><IcyBlue>/\\\\\\</><SoftOrchid>_______________</><IcyBlue>\\/\\\\\\</><SoftOrchid>___________________</><IcyBlue>\\/\\\\\\</><SoftOrchid>_</>     ");
        WriteLineWithColorMarkup("    <SoftOrchid>________</><IcyBlue>\\////\\\\\\<SoftOrchid>__</><IcyBlue>\\///\\\\\\/\\\\\\/</><SoftOrchid>_____________</><IcyBlue>/\\\\\\\\\\\\\\\\\\\\\\</><SoftOrchid>_______________</><IcyBlue>\\/\\\\\\</><SoftOrchid>_</>    ");
        WriteLineWithColorMarkup("     <SoftOrchid>___________</><IcyBlue>\\//\\\\\\</><SoftOrchid>___</><IcyBlue>\\///\\\\\\/</><SoftOrchid>______________</><IcyBlue>\\/////\\\\\\///</><SoftOrchid>________________</><IcyBlue>\\/\\\\\\</><SoftOrchid>_</>   ");
        WriteLineWithColorMarkup("      <SoftOrchid>__</><IcyBlue>/\\\\\\</><SoftOrchid>______</><IcyBlue>/\\\\\\</><SoftOrchid>_____</><IcyBlue>/\\\\\\/\\\\\\</><SoftOrchid>_________________</><IcyBlue>\\/\\\\\\</IcyBlue><SoftOrchid>___________________</><IcyBlue>\\/\\\\\\</><SoftOrchid>_</>  ");
        WriteLineWithColorMarkup("       <SoftOrchid>_</><IcyBlue>\\///\\\\\\\\\\\\\\\\\\/</><SoftOrchid>____</><IcyBlue>/\\\\\\/\\///\\\\\\</><SoftOrchid>_______________</><IcyBlue>\\///</><SoftOrchid>____________________</><IcyBlue>\\/\\\\\\</><SoftOrchid>_ </> ");
        WriteLineWithColorMarkup("        <SoftOrchid>___</><IcyBlue>\\/////////</><SoftOrchid>_____</><IcyBlue>\\///</><SoftOrchid>____</><IcyBlue>\\///</><SoftOrchid>________________________________________</><IcyBlue>\\///</><SoftOrchid>__ </> ");
        WriteLine("");
    }

    /// <summary>
    /// Write a spinning bar to the console in a threadsafe way to indicate an ongoing process.
    /// </summary>
    public async Task StartSpinningBar(string? message = null)
    {
        long previousMilliseconds = 0;
        const long updateInterval = 100;

        _cancellationTokenSource = new CancellationTokenSource();

        Stopwatch stopwatch = Stopwatch.StartNew();

        SetCursorVisibility(false);

        if (message != null)
        {
            WriteWithColorMarkup($"{message}");
        }

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            long currentMilliseconds = stopwatch.ElapsedMilliseconds;

            if (currentMilliseconds - previousMilliseconds >= updateInterval)
            {
                WriteWithColorMarkup($"{_spinner[_spinnerCounter]}");
                SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
                _spinnerCounter = (_spinnerCounter + 1) % _spinner.Length;
                previousMilliseconds = currentMilliseconds;
            }

            await Task.Delay(1, _cancellationTokenSource.Token);
        }

        stopwatch.Stop();
    }

    /// <summary>
    /// Stop the spinning bar in a threadsafe way.
    /// </summary>
    public async Task StopSpinningBar(string? message = null)
    {
        if (_cancellationTokenSource == null)
        {
            return;
        }

        await _cancellationTokenSource.CancelAsync();

        SetCursorPosition(Console.CursorLeft - (message?.Length ?? 0), Console.CursorTop);
        Write(new string(' ', message?.Length + 1 ?? 1));
        SetCursorPosition(Console.CursorLeft - (message?.Length + 1 ?? 1), Console.CursorTop);

        SetCursorVisibility(true);

        _cancellationTokenSource = null;
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

    #region Windows API imports

    private const int STD_OUTPUT_HANDLE = -11;
    private const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetStdHandle(int nStdHandle);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool GetConsoleMode(IntPtr hConsoleHandle, out uint lpMode);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool SetConsoleMode(IntPtr hConsoleHandle, uint dwMode);

    /// <summary>
    /// Enable ANSI support for the console if running on Windows.
    /// </summary>
    private static void EnableAnsiSupport()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            var handle = GetStdHandle(STD_OUTPUT_HANDLE);
            if (!GetConsoleMode(handle, out uint mode))
            {
                return;
            }

            // Enable ANSI escape codes
            mode |= ENABLE_VIRTUAL_TERMINAL_PROCESSING;

            if (!SetConsoleMode(handle, mode))
            {
                return;
            }

            return;
        }
        catch
        {
            return;
        }
    }

    #endregion
}