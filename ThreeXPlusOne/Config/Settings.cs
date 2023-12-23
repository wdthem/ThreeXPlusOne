using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

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
    private static readonly char[] _separator = [','];

    /// <summary>
    /// The width of the SkiaSharp canvas in pixels
    /// </summary>
    public int CanvasWidth { get; set; } = 30000;

    /// <summary>
    /// The height of the SkiaSharp canvas in pixels
    /// </summary>
    public int CanvasHeight { get; set; } = 35000;

    /// <summary>
    /// The amount of numbers to randomly generate to run through the algorithm
    /// </summary>
    public int NumberOfSeries { get; set; } = 200;

    /// <summary>
    /// The max value that the randomly selected numbers can be
    /// </summary>
    public int MaxStartingNumber { get; set; } = 1000;

    /// <summary>
    /// If supplied, the algorithm will be run only on these numbers
    /// </summary>
    public string UseTheseNumbers { get; set; } = "";

    /// <summary>
    /// If supplied, these numbers will be excluded from either random number selection or from the UseTheseNumbers property
    /// </summary>
    public string ExcludeTheseNumbers { get; set; } = "";

    /// <summary>
    /// The angle by which a node will be rotated by on the graph
    /// </summary>
    /// <remarks>
    /// If the node value is even, it is rotated clockwise. If odd, anti-clockwise
    /// </remarks>
    public double NodeRotationAngle { get; set; } = 0;

    /// <summary>
    /// The radius of the node
    /// </summary>
    public float NodeRadius { get; set; } = 40;

    /// <summary>
    /// Whether or not to distort nodes into various shapes and sizes
    /// </summary>
    public bool DistortNodes { get; set; }

    /// <summary>
    /// The max amount in pixels by which to distort the node's radius
    /// </summary>
    public int RadiusDistortion { get; set; } = 30;

    /// <summary>
    /// The amount of x-axis space in pixels by which to separate nodes
    /// </summary>
    public int XNodeSpacer { get; set; } = 125;

    /// <summary>
    /// The amount of y-axis space in pixels by which to separate nodes
    /// </summary>
    public int YNodeSpacer { get; set; } = 125;

    /// <summary>
    /// For pseudo-3D graphs, the distance from the viewer (to create perspective)
    /// </summary>
    public float DistanceFromViewer { get; set; } = 200;

    /// <summary>
    /// The number of dimensions to render in the graph
    /// </summary>
    /// <remarks>2 or 3</remarks>
    public int GraphDimensions { get; set; } = 2;

    /// <summary>
    /// Whether or not to draw the connections between nodes on the graph
    /// </summary>
    public bool DrawConnections { get; set; } = true;

    /// <summary>
    /// Whether or not to draw the number at the center of the node that the node represents
    /// </summary>
    public bool DrawNumbersOnNodes { get; set; } = true;

    /// <summary>
    /// Whether or not to generate the directed graph
    /// </summary>
    public bool GenerateGraph { get; set; }

    /// <summary>
    /// Whether or not to generate the histogram illustrating Benford's law
    /// </summary>
    public bool GenerateHistogram { get; set; }

    /// <summary>
    /// Whether or not to generate the metadata file
    /// </summary>
    public bool GenerateMetadataFile { get; set; }

    /// <summary>
    /// Whether or not to draw stars on the graph's background
    /// </summary>
    public bool GenerateBackgroundStars { get; set; }

    /// <summary>
    /// The directory in which the process will create a unique execution folder with generated output
    /// </summary>
    public string OutputPath { get; set; } = "";

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

            foreach (var numberAsString in stringArray)
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
        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(string.Join("", ListOfSeriesNumbers.OrderBy(x => x))));

        StringBuilder builder = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString().ToLower();
    }
}