﻿using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.App.Config;

/// <summary>
/// Settings supplied by the user to control aspects of the given execution of the process.
/// </summary>
/// <remarks>
/// If no app settings file is supplied by the user, the defaults set here will be used.
/// </remarks>
public class AppSettings
{
    private string? _uniqueExecutionId;

    private readonly GraphProvider _graphProvider = GraphProvider.SkiaSharp;

    /// <summary>
    /// All settings related to running the 3x+1 algorithm.
    /// </summary>
    public AlgorithmSettings AlgorithmSettings { get; set; } = new();

    /// <summary>
    /// All settings related to the appearance of the directed graph nodes.
    /// </summary>
    public NodeAestheticSettings NodeAestheticSettings { get; set; } = new();

    /// <summary>
    /// All settings related to the appears of the directed graph itself.
    /// </summary>
    public DirectedGraphAestheticSettings DirectedGraphAestheticSettings { get; set; } = new();

    /// <summary>
    /// All settings related to the specific instance type of the directed graph.
    /// </summary>
    public DirectedGraphInstanceSettings DirectedGraphInstanceSettings { get; set; } = new();

    /// <summary>
    /// The file type to save the generated graph as.
    /// </summary>
    [AppSetting(description: "The file type of the generated image. Values are: {ImageTypesPlaceholder}", suggestedValue: "Jpeg")]
    public string OutputFileType { get; set; } = "Jpeg";

    /// <summary>
    /// The quality of the generated image, on a scale of 1 to 100. 100 is the best quality, 1 is the lowest.
    /// </summary>
    [AppSetting(description: "The quality of the generated image, on a scale of 1 to 100. 100 is the best quality, and 1 is the lowest.", suggestedValue: "100")]
    public int OutputFileQuality { get; set; } = 100;

    /// <summary>
    /// The directory in which the process will create a unique execution folder with generated output.
    /// </summary>
    [AppSetting(description: "The folder in which the generated output images should be placed. If not supplied, output is saved to the runtime directory.", suggestedValue: "")]
    public string OutputPath { get; set; } = "";

    /// <summary>
    /// The graph provider to use to render the graph image.
    /// </summary>
    [JsonIgnore]
    public GraphProvider GraphProvider
    {
        get
        {
            return _graphProvider;
        }
    }

    /// <summary>
    /// The name of the file in which these app settings are stored.
    /// </summary>
    [JsonIgnore]
    public string SettingsFileName { get; set; } = "";

    /// <summary>
    /// The full path to the app settings file (could be provided by the user).
    /// </summary>
    [JsonIgnore]
    public string SettingsFileFullPath { get; set; } = "";

    /// <summary>
    /// An MD5 hash used to name a directory to store the output for the run of the given number series.
    /// </summary>
    [JsonIgnore]
    public string UniqueExecutionId
    {
        get
        {
            if (string.IsNullOrEmpty(_uniqueExecutionId))
            {
                _uniqueExecutionId = ComputeHashFromSeriesData();
            }

            return _uniqueExecutionId;
        }
    }

    /// <summary>
    /// Create an MD5 hash of the list of numbers that the process is running on such that
    /// the output directory can be uniquely named and re-used if the process is re-run with the same numbers.
    /// </summary>
    /// <returns></returns>
    private string ComputeHashFromSeriesData()
    {
        List<int> copyOfSeriesNumbers = AlgorithmSettings.ListOfNumbersToUse;
        copyOfSeriesNumbers.RemoveAll(AlgorithmSettings.ListOfNumbersToExclude.Contains);

        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(string.Join("", copyOfSeriesNumbers.OrderBy(x => x))));

        StringBuilder builder = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString().ToLower();
    }
}

public partial class AlgorithmSettings
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();

    /// <summary>
    /// If supplied, these numbers will be excluded from either random number selection or from the UseTheseNumbers property.
    /// </summary>
    [AppSetting(description: "A comma-separated list of numbers not to use.", suggestedValue: "")]
    public string NumbersToExclude { get; set; } = "";

    /// <summary>
    /// If supplied, the algorithm will be run only on these numbers.
    /// </summary>
    [AppSetting(description: $"A comma-separated list of numbers to run the program with. Overrides {nameof(RandomNumberTotal)} and {nameof(RandomNumberMax)}.", suggestedValue: "")]
    public string NumbersToUse { get; set; } = "";

    /// <summary>
    /// The max value that the randomly selected numbers can be.
    /// </summary>
    [AppSetting(description: "The highest possible starting number of any given series.", suggestedValue: "1000")]
    public int RandomNumberMax { get; set; } = 1000;

    /// <summary>
    /// The amount of numbers to randomly generate to run through the algorithm.
    /// </summary>
    [AppSetting(description: "The total number of series that will be generated by the algorithm.", suggestedValue: "200")]
    public int RandomNumberTotal { get; set; } = 200;

    /// <summary>
    /// Whether or not to use the Collatz Conjecture algorithm, which improves algorithm speed, and reduces the output of values.
    /// </summary>
    /// <remarks>
    /// See https://en.wikipedia.org/wiki/Collatz_conjecture
    /// </remarks>
    [AppSetting(description: "Whether or not to use the Collatz Conjecture shortcut algorithm", suggestedValue: "false")]
    public bool UseShortcutAlgorithm { get; set; } = false;

    /// <summary>
    /// The supplied number list in NumbersToUse as a list of integers.
    /// </summary>
    [JsonIgnore]
    public List<int> ListOfNumbersToUse
    {
        get
        {
            List<int> parsedNumbers = [];

            if (string.IsNullOrWhiteSpace(NumbersToUse))
            {
                return parsedNumbers;
            }

            string result = WhiteSpaceRegex().Replace(NumbersToUse, "");

            return result.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(n => int.TryParse(n.Trim(), out int result) ? result : (int?)null)
                         .Where(n => n.HasValue && n > 0)
                         .Select(n => n!.Value)
                         .Distinct()
                         .ToList();
        }
    }

    /// <summary>
    /// The numbers to exclude parsed as a list of integers.
    /// </summary>
    [JsonIgnore]
    public List<int> ListOfNumbersToExclude
    {
        get
        {
            List<int> parsedNumbers = [];

            if (string.IsNullOrWhiteSpace(NumbersToExclude))
            {
                return parsedNumbers;
            }

            string result = WhiteSpaceRegex().Replace(NumbersToExclude, "");

            return result.Split(',', StringSplitOptions.RemoveEmptyEntries)
                         .Select(n => int.TryParse(n.Trim(), out int result) ? result : (int?)null)
                         .Where(n => n.HasValue && n > 0)
                         .Select(n => n!.Value)
                         .Distinct()
                         .ToList();
        }
    }

    /// <summary>
    /// Whether or not the algorithm was run with random numbers.
    /// </summary>
    [JsonIgnore]
    public bool FromRandomNumbers { get; set; } = false;
}

public class NodeAestheticSettings
{
    /// <summary>
    /// Whether or not to draw the number at the center of the node that the node represents.
    /// </summary>
    [AppSetting(description: $"Whether or not to use the same colour for numbers generated as part of the same series. If {nameof(NodeColors)} is supplied, uses only those colours, otherwise uses randomly-selected colours.", suggestedValue: "false")]
    public bool ColorCodeNumberSeries { get; set; } = true;

    /// <summary>
    /// Whether or not to draw the connections between nodes on the graph.
    /// </summary>
    [AppSetting(description: "Whether or not to draw connections between the nodes in the graph. If set to true, image file size may increase substantially.", suggestedValue: "true")]
    public bool DrawNodeConnections { get; set; } = true;

    /// <summary>
    /// Whether or not to draw the number at the center of the node that the node represents.
    /// </summary>
    [AppSetting(description: "Whether or not to draw the number at the center of the node that the node represents.", suggestedValue: "true")]
    public bool DrawNumbersOnNodes { get; set; } = true;

    /// <summary>
    /// Hex values to be used for node colours (include the '#' with each code).
    /// </summary>
    [AppSetting(description: $"Comma-separated list of hex codes to use as colours for the nodes (include the '#' with each code). Leave blank to use randomly-selected colours.", suggestedValue: "")]
    public string NodeColors { get; set; } = "";

    /// <summary>
    /// The radius of the node.
    /// </summary>
    [AppSetting(description: "The radius of the nodes in pixels.", suggestedValue: "50 for 2D, 275 for 3D")]
    public double NodeRadius { get; set; } = 50;

    /// <summary>
    /// The angle by which a node will be rotated by on the graph.
    /// </summary>
    /// <remarks>
    /// If the node value is even, it is rotated clockwise. If odd, anti-clockwise.
    /// </remarks>
    [AppSetting(description: "The magnitude of the rotation angle. 0 is no rotation. When using rotation, start small, such as 0.8", suggestedValue: "0")]
    public double NodeRotationAngle { get; set; } = 0;

    /// <summary>
    /// Shapes to use to draw nodes.
    /// </summary>
    [AppSetting(description: "Comma-separated list of shapes to use for drawing nodes. Values are: {ShapesPlaceholder}. Leave blank to use randomly-selected shapes.", suggestedValue: "")]
    public string NodeShapes { get; set; } = "";

    /// <summary>
    /// The amount of x-axis space in pixels by which to separate nodes.
    /// </summary>
    [AppSetting(description: "The default space between nodes on the x-axis.", suggestedValue: "125 for 2D, 250 for 3D")]
    public int NodeSpacerX { get; set; } = 125;

    /// <summary>
    /// The amount of y-axis space in pixels by which to separate nodes.
    /// </summary>
    [AppSetting(description: "The default space between nodes on the y-axis.", suggestedValue: "125 for 2D, 225 for 3D")]
    public int NodeSpacerY { get; set; } = 125;
}

public class DirectedGraphAestheticSettings
{
    /// <summary>
    /// Hex code for the background colour of the canvas (include the '#' with the code).
    /// </summary>
    [AppSetting(description: "The hex code for the background colour of the canvas (include the '#' with the code). Leave blank for black.", suggestedValue: "")]
    public string CanvasColor { get; set; } = "";

    /// <summary>
    /// The type of the graph that will be rendered.
    /// </summary>
    [AppSetting(description: "The type of the graph that will be rendered. Values are: {GraphTypePlaceholder}", suggestedValue: "Standard2D")]
    public string GraphType { get; set; } = "Standard2D";

    /// <summary>
    /// The colour of the light source (include the '#' with the code). Leave blank for default "LightYellow".
    /// </summary>
    [AppSetting(description: "The hex code for the colour of the light source (include the '#' with the code). Leave blank for Light Yellow.", suggestedValue: "")]
    public string LightSourceColor { get; set; } = "";

    /// <summary>
    /// The position of the light source. Use "None" to not generate the light source.
    /// </summary>
    [AppSetting(description: "The position of the light source. Values are: {LightSourcePositionsPlaceholder}", suggestedValue: "None")]
    public string LightSourcePosition { get; set; } = "None";
}

public class DirectedGraphInstanceSettings
{
    /// <summary>
    /// For the standard pseudo-3D graph, the distance from the viewer (to create perspective).
    /// </summary>
    [AppSetting(description: "For the Standard Pseudo-3D graph, the distance from the viewer. Used when applying the perspective transformation.", suggestedValue: "200")]
    public double Pseudo3DViewerDistance { get; set; } = 200;

    /// <summary>
    /// For Radial and Galaxy graphs, the spacing between radial layers.
    /// </summary>
    [AppSetting(description: "For Radial and Galaxy graphs, the spacing between radial layers", suggestedValue: "25")]
    public int RadialLayerSpacing { get; set; } = 25;

    /// <summary>
    /// For Spiral and Galaxy graphs, the angle of the spiral. Lower numbers are more spiralled, higher numbers are more polygonal / shape-like.
    /// </summary>
    [AppSetting(description: "For Spiral and Galaxy graphs, the angle of the spiral. Lower numbers are more spiralled, higher numbers are more polygonal / shape-like.", suggestedValue: "70")]
    public double SpiralAngle { get; set; } = 70;
}