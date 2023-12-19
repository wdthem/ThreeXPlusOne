using System.Security.Cryptography;
using System.Text;

namespace ThreeXPlusOne.Config;

/// <summary>
/// Run --help command for property documentation
/// Defaults are provided in this class
/// </summary>
public class Settings
{
    private string? _uniqueExecutionId;
    private static readonly char[] _separator = [','];

    public int CanvasWidth { get; set; } = 30000;
    public int CanvasHeight { get; set; } = 35000;
    public int NumberOfSeries { get; set; } = 200;
    public int MaxStartingNumber { get; set; } = 1000;
    public string UseTheseNumbers { get; set; } = "";
    public string ExcludeTheseNumbers { get; set; } = "";
    public double NodeRotationAngle { get; set; } = 0;
    public float NodeRadius { get; set; } = 40;
    public bool DistortNodes { get; set; }
    public int RadiusDistortion { get; set; } = 30;
    public int XNodeSpacer { get; set; } = 125;
    public int YNodeSpacer { get; set; } = 125;
    public float DistanceFromViewer { get; set; } = 200;
    public int GraphDimensions { get; set; } = 2;
    public bool GenerateGraph { get; set; }
    public bool GenerateHistogram { get; set; }
    public bool GenerateMetadataFile { get; set; }
    public bool GenerateBackgroundStars { get; set; }
    public string OutputPath { get; set; } = "";

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

    public int ParsedGraphDimensions
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