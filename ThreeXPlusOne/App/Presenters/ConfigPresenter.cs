using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;

namespace ThreeXPlusOne.App.Presenters;

public partial class ConfigPresenter(IOptions<AppSettings> appSettings,
                                     IConsoleService consoleService,
                                     IUiComponent uiComponent) : IConfigPresenter
{
    private readonly AppSettings _appSettings = appSettings.Value;

    [GeneratedRegex("([a-z])([A-Z])")]
    private static partial Regex SplitToWordsRegex();

    /// <summary>
    /// Write info about the app's config settings to the console.
    /// </summary>
    public void WriteConfigText()
    {
        uiComponent.WriteHeading("Configuration");

        uiComponent.WriteHeading("App settings");
        consoleService.WriteLine("If no custom app settings are supplied, defaults will be used.\n");
        consoleService.WriteLine($"To apply custom app settings, place a file called '{_appSettings.SettingsFileName}' in the same folder as the executable. Or use the --settings flag to provide a directory path to the '{_appSettings.SettingsFileName}' file.\n\nIt must have the following content:\n");

        consoleService.WriteLineWithColorMarkup("[BlushRed]{[/]");

        WriteSettings(type: null,
                      instance: null,
                      sectionName: null,
                      includeHeader: false,
                      isJson: true);

        consoleService.WriteLineWithColorMarkup("[BlushRed]}[/]");

        uiComponent.WriteHeading("Definitions and suggested values");

        List<string> lines = GenerateSuggestedValueText();

        lines.Add("\nThe above app settings are a good starting point from which to experiment.\n");
        lines.Add("Alternatively, start with the app settings from the Example Output on the GitHub repository: https://github.com/wdthem/ThreeXPlusOne/blob/main/ThreeXPlusOne.ExampleOutput/ExampleOutputSettings.txt\n");

        ScrollOutput("app settings", lines);

        uiComponent.WriteHeading("Performance");
        consoleService.WriteLine("Be aware that increasing some app settings may result in large canvas sizes, which could cause the program to fail. It depends on the capabilities of the machine running it.\n\n");
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
            uiComponent.WriteHeading("Settings");
        }

        if (!string.IsNullOrWhiteSpace(sectionName))
        {
            string sectionNameWords = isJson
                                        ? $"\"{sectionName}\":"
                                        : $"{SplitToWordsRegex().Replace(sectionName, "$1 $2")}:";

            if (isJson)
            {
                consoleService.WriteLineWithColorMarkup($"    [AquaTeal]{sectionNameWords}[/]");
                consoleService.WriteLineWithColorMarkup("[BlushRed]{[/]");
            }
            else
            {
                consoleService.WriteLineWithColorMarkup($"    [AquaTeal]{sectionNameWords}[/]");
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
                    consoleService.WriteLineWithColorMarkup("    [BlueTint]General Settings:[/]");

                    generalSettingsWritten = true;
                }

                object? value = property.GetValue(instance, null);

                if (isJson)
                {
                    if (type != typeof(AppSettings))
                    {
                        consoleService.Write("    ");
                    }

                    consoleService.WriteWithColorMarkup($"    [IcyBlue]\"{property.Name}\":[/] ");
                }
                else
                {
                    consoleService.WriteWithColorMarkup($"        [IcyBlue]{property.Name}:[/] ");
                }

                if ((value?.ToString() ?? "").Length > 100)
                {
                    value = TruncateLongSettings(value?.ToString() ?? "");
                }

                if (!isJson)
                {
                    consoleService.Write($"{value}");
                }
                else
                {
                    if (property.PropertyType == typeof(string))
                    {
                        consoleService.WriteWithColorMarkup("[WarmSand]\"[value]\"[/]");
                    }
                    else
                    {
                        consoleService.WriteWithColorMarkup("[WarmSand][value][/]");
                    }

                    if (lcv < appSettingsProperties.Count)
                    {
                        consoleService.Write(",");
                    }
                }

                consoleService.WriteLine("");
            }

            lcv++;
        }

        if (isJson && type != typeof(AppSettings))
        {
            consoleService.WriteLineWithColorMarkup("    [BlushRed]},[/]");
        }
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
    /// Allow the user to scroll through the settings output, as it is longer than a standard console window's height.
    /// </summary>
    /// <param name="outputType"></param>
    /// <param name="lines"></param>
    private void ScrollOutput(string outputType, List<string> lines)
    {
        int currentLine = 0;

        consoleService.WriteLine($"\nPress a key to scroll {outputType} text, 'q' to output all...");

        int startLeft = Console.CursorLeft;
        int startTop = Console.CursorTop;

        while (true)
        {
            if (Console.KeyAvailable)
            {
                ConsoleKey key = Console.ReadKey(intercept: true).Key;

                if (currentLine == 0)
                {
                    consoleService.SetCursorPosition(startLeft, startTop - 1);
                    consoleService.Write(new string(' ', Console.WindowWidth)); // Overwrite the line with spaces
                    consoleService.SetCursorPosition(startLeft, startTop - 1);
                }

                if (key == ConsoleKey.Q)
                {
                    //output all remaining lines in one go
                    for (int lcv = currentLine; lcv < lines.Count; lcv++)
                    {
                        consoleService.WriteLineWithColorMarkup(lines[lcv]);
                    }

                    break;
                }

                if (currentLine < lines.Count)
                {
                    consoleService.WriteLineWithColorMarkup(lines[currentLine]);

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

            lines.Add($"[AquaTeal]{sectionNameWords}[/]");
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
                    lines.Add($"[AquaTeal]General Settings[/]\n");

                    generalSettingsWritten = true;
                }

                lines.Add($"  [IcyBlue]{property.Name}[/]");

                AppSettingAttribute? settingAttribute = property.GetCustomAttribute<AppSettingAttribute>();

                if (settingAttribute != null)
                {
                    lines.Add($"  {settingAttribute.Description.Replace("{LightSourcePositionsPlaceholder}", string.Join(", ", Enum.GetNames(typeof(LightSourcePosition)).OrderBy(name => name)))
                                                               .Replace("{ImageTypesPlaceholder}", string.Join(", ", Enum.GetNames(typeof(ImageType)).OrderBy(name => name)))
                                                               .Replace("{GraphTypePlaceholder}", string.Join(", ", Enum.GetNames(typeof(GraphType)).OrderBy(name => name)))
                                                               .Replace("{ShapesPlaceholder}", string.Join(", ", Enum.GetNames(typeof(ShapeType)).OrderBy(name => name)))}");

                    string suggestedValueText = "  [BlueTint]Suggested value:[/] ";

                    if (property.PropertyType == typeof(string))
                    {
                        suggestedValueText += $"[WarmSand]\"{settingAttribute.SuggestedValue}\"[/]\n";
                    }
                    else
                    {
                        suggestedValueText += $"[WarmSand]{settingAttribute.SuggestedValue}[/]\n";
                    }

                    lines.Add(suggestedValueText);
                }
            }
        }

        return lines;
    }
}