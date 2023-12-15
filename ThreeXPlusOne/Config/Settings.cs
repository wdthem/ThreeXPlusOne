﻿using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace ThreeXPlusOne.Config;

public class Settings
{
    [JsonIgnore]
    private string? _uniqueExecutionId;
    
    public int CanvasWidth { get; set; }
    public int CanvasHeight { get; set; }
    public int NumberOfSeries { get; set; }
    public int MaxStartingNumber { get; set; }
    public string UseTheseNumbers { get; set; } = "";
    public string ExcludeTheseNumbers { get; set; } = "";
    public double NodeRotationAngle { get; set; }
    public float NodeRadius { get; set; }
    public bool DistortNodes { get; set; }
    public int RadiusDistortion { get; set; }
    public int XNodeSpacer { get; set; }
    public int YNodeSpacer { get; set; }
    public int GraphDimensions { get; set; }
    public bool GenerateGraph { get; set; }
    public bool GenerateHistogram { get; set; }
    public bool GenerateMetadataFile { get; set; }
    public bool GenerateBackgroundStars { get; set; }
    public string OutputPath { get; set; } = "";

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

    [JsonIgnore]
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

    [JsonIgnore]
    public List<int> ListOfSeriesNumbers
    {
        get
        {
            var parsedNumbers = new List<int>();

            if (string.IsNullOrEmpty(UseTheseNumbers))
            {
                return parsedNumbers;
            }

            string[] stringArray = UseTheseNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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

    [JsonIgnore]
    public List<int> ListOfNumbersToExclude
    {
        get
        {
            var parsedNumbers = new List<int>();

            if (string.IsNullOrEmpty(ExcludeTheseNumbers))
            {
                return parsedNumbers;
            }

            string[] stringArray = ExcludeTheseNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

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

    private string ComputeHashFromSeriesData()
    {
        byte[] bytes = MD5.HashData(Encoding.UTF8.GetBytes(string.Join("", ListOfSeriesNumbers)));

        StringBuilder builder = new();

        for (int i = 0; i < bytes.Length; i++)
        {
            builder.Append(bytes[i].ToString("x2"));
        }

        return builder.ToString().ToLower();
    }
}