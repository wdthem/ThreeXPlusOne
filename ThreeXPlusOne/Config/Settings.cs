﻿using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using ThreeXPlusOne.App.Enums;

namespace ThreeXPlusOne.Config;

/// <summary>
/// Run --help command for additional property documentation
/// If no settings file is supplied, the defaults set here would be used
/// </summary>
public class Settings
{
    [JsonIgnore]
    private string? _uniqueExecutionId;

    [JsonIgnore]
    private readonly string _settingsFileName = "settings.json";

    [JsonIgnore]
    private readonly GraphProvider _graphProvider = GraphProvider.SkiaSharp;

    [JsonIgnore]
    private static readonly char[] _separator = [','];

    /// <summary>
    /// The amount of numbers to randomly generate to run through the algorithm
    /// </summary>
    [SettingInfo(description: "The total number of series that will run", suggestedValue: "200")]
    public int NumberOfSeries { get; set; } = 200;

    /// <summary>
    /// The max value that the randomly selected numbers can be
    /// </summary>
    [SettingInfo(description: "The highest number any given series can start with", suggestedValue: "1000")]
    public int MaxStartingNumber { get; set; } = 1000;

    /// <summary>
    /// If supplied, the algorithm will be run only on these numbers
    /// </summary>
    [SettingInfo(description: $"Comma-separated list of numbers to run the program with. Overrides {nameof(NumberOfSeries)} and {nameof(MaxStartingNumber)}", suggestedValue: "")]
    public string UseTheseNumbers { get; set; } = "";

    /// <summary>
    /// If supplied, these numbers will be excluded from either random number selection or from the UseTheseNumbers property
    /// </summary>
    [SettingInfo(description: "Comma-separated list of numbers not to use", suggestedValue: "")]
    public string ExcludeTheseNumbers { get; set; } = "";

    /// <summary>
    /// The angle by which a node will be rotated by on the graph
    /// </summary>
    /// <remarks>
    /// If the node value is even, it is rotated clockwise. If odd, anti-clockwise
    /// </remarks>
    [SettingInfo(description: "The size of the rotation angle. 0 is no rotation. When using rotation, start small, such as 0.8", suggestedValue: "0")]
    public double NodeRotationAngle { get; set; } = 0;

    /// <summary>
    /// The radius of the node
    /// </summary>
    [SettingInfo(description: "The radius of the nodes in pixels", suggestedValue: "50 for 2D, 275 for 3D")]
    public double NodeRadius { get; set; } = 50;

    /// <summary>
    /// Whether or not to draw polygons as nodes in addition to circles
    /// </summary>
    [SettingInfo(description: "Whether or not to use circles or polygons + circles as graph nodes", suggestedValue: "false")]
    public bool IncludePolygonsAsNodes { get; set; }

    /// <summary>
    /// The amount of x-axis space in pixels by which to separate nodes
    /// </summary>
    [SettingInfo(description: "The space between nodes on the x-axis", suggestedValue: "125 for 2D, 250 for 3D")]
    public int XNodeSpacer { get; set; } = 125;

    /// <summary>
    /// The amount of y-axis space in pixels by which to separate nodes
    /// </summary>
    [SettingInfo(description: "The space between nodes on the y-axis", suggestedValue: "125 for 2D, 225 for 3D")]
    public int YNodeSpacer { get; set; } = 125;

    /// <summary>
    /// For pseudo-3D graphs, the distance from the viewer (to create perspective)
    /// </summary>
    [SettingInfo(description: "For the 3D graph, the distance from the view when applying the perspective transformation", suggestedValue: "200")]
    public double DistanceFromViewer { get; set; } = 200;

    /// <summary>
    /// The number of dimensions to render in the graph
    /// </summary>
    /// <remarks>2 or 3</remarks>
    [SettingInfo(description: "The number of dimensions to render in the graph - 2 or 3", suggestedValue: "2")]
    public int GraphDimensions { get; set; } = 2;

    /// <summary>
    /// The position of the light source. Use "None" to not generate the light source.
    /// </summary>
    [SettingInfo(description: "The position of the light source. Values are: {LightSourcePositionsPlaceholder}", suggestedValue: "None")]
    public string LightSourcePosition { get; set; } = "None";

    /// <summary>
    /// Whether or not to draw the connections between nodes on the graph
    /// </summary>
    [SettingInfo(description: "Whether or not to draw connections between the nodes in the graph - if true can increase image file size substantially", suggestedValue: "true")]
    public bool DrawConnections { get; set; } = true;

    /// <summary>
    /// Whether or not to draw the number at the center of the node that the node represents
    /// </summary>
    [SettingInfo(description: "Whether or not to draw the numbers at the center of the node that the node represents", suggestedValue: "true")]
    public bool DrawNumbersOnNodes { get; set; } = true;

    /// <summary>
    /// Whether or not to generate the directed graph
    /// </summary>
    [SettingInfo(description: "Whether or not to generate the visualization of the data", suggestedValue: "true")]
    public bool GenerateGraph { get; set; } = true;

    /// <summary>
    /// Whether or not to generate the histogram illustrating Benford's law
    /// </summary>
    [SettingInfo(description: "Whether or not to generate a histogram of the distribution of numbers starting from 1-9", suggestedValue: "true")]
    public bool GenerateHistogram { get; set; } = true;

    /// <summary>
    /// Whether or not to generate the metadata file
    /// </summary>
    [SettingInfo(description: "Whether or not to generate a file with metadata about the run", suggestedValue: "true")]
    public bool GenerateMetadataFile { get; set; } = true;

    /// <summary>
    /// Whether or not to draw stars on the graph's background
    /// </summary>
    [SettingInfo(description: "Whether or not to generate random stars in the background of the graph", suggestedValue: "false")]
    public bool GenerateBackgroundStars { get; set; }

    /// <summary>
    /// The directory in which the process will create a unique execution folder with generated output
    /// </summary>
    [SettingInfo(description: "The folder in which the output files should be placed", suggestedValue: "C:\\path\\to\\save\\image\\")]
    public string OutputPath { get; set; } = "";

    /// <summary>
    /// The graph provider to use to render the graph image
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
    /// The name of the file in which these settings are stored
    /// </summary>
    [JsonIgnore]
    public string SettingsFileName
    {
        get
        {
            return _settingsFileName;
        }
    }

    /// <summary>
    /// An MD5 hash used to name a directory to store the output for the run of the given number series
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
    /// The sanitized graph dimensions property
    /// </summary>
    [JsonIgnore]
    public int SanitizedGraphDimensions
    {
        get
        {
            if (GraphDimensions < 2 || GraphDimensions > 3)
            {
                return 2;
            }

            return GraphDimensions;
        }
    }

    /// <summary>
    /// The series numbers parsed as a list of integers
    /// </summary>
    [JsonIgnore]
    public List<int> ListOfSeriesNumbers
    {
        get
        {
            var parsedNumbers = new List<int>();

            if (string.IsNullOrWhiteSpace(UseTheseNumbers))
            {
                return parsedNumbers;
            }

            string[] stringArray = UseTheseNumbers.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (string numberAsString in stringArray)
            {
                if (int.TryParse(numberAsString, out int parsedNumber))
                {
                    if (parsedNumber <= 0)
                    {
                        continue;
                    }

                    parsedNumbers.Add(parsedNumber);
                }
            }

            return parsedNumbers;
        }
    }

    /// <summary>
    /// The number to exclude parsed as a list of integers
    /// </summary>
    [JsonIgnore]
    public List<int> ListOfNumbersToExclude
    {
        get
        {
            var parsedNumbers = new List<int>();

            if (string.IsNullOrWhiteSpace(ExcludeTheseNumbers))
            {
                return parsedNumbers;
            }

            string[] stringArray = ExcludeTheseNumbers.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

            foreach (var numberAsString in stringArray)
            {
                if (int.TryParse(numberAsString, out int parsedNumber))
                {
                    parsedNumbers.Add(parsedNumber);
                }
            }

            return parsedNumbers;
        }
    }

    /// <summary>
    /// Create an MD5 hash of the list of numbers that the process is running on such that
    /// the output directory can be uniquely named and re-used if the process is re-run with the same numbers
    /// </summary>
    /// <returns></returns>
    private string ComputeHashFromSeriesData()
    {
        List<int> copyOfSeriesNumbers = ListOfSeriesNumbers;
        copyOfSeriesNumbers.RemoveAll(ListOfNumbersToExclude.Contains);

        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(string.Join("", copyOfSeriesNumbers.OrderBy(x => x))));

        StringBuilder builder = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString().ToLower();
    }
}