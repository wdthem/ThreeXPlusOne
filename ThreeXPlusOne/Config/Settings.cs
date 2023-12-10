
using System.Text.Json.Serialization;

namespace ThreeXPlusOne.Config;

public class Settings
{
    public int CanvasWidth { get; set; }
    public int CanvasHeight { get; set; }
    public int NumberOfSeries { get; set; }
    public int MaxStartingNumber { get; set; }
    public string? NumbersToExclude { get; set; }
    public double RotationAngle { get; set; }
    public int XNodeSpacer { get; set; }
    public int YNodeSpacer { get; set; }
    public bool GenerateGraph { get; set; }
    public bool GenerateHistogram { get; set; }
    public string? OutputPath { get; set; }

    [JsonIgnore]
    public string FileNameUniqueId { get; set; } = Guid.NewGuid().ToString();

    [JsonIgnore]
    public List<int> ListOfNumbersToExclude
    {
        get
        {
            var parsedNumbers = new List<int>();

            if (string.IsNullOrEmpty(NumbersToExclude))
            {
                return parsedNumbers;
            }

            string[] stringArray = NumbersToExclude.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            foreach(var numberAsString in stringArray)
            {
                if (int.TryParse(numberAsString, out int parsedNumber))
                {
                    parsedNumbers.Add(parsedNumber);
                }
            }

            return parsedNumbers;
        }
    }
}