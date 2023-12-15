using System.Diagnostics;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Process : IProcess
{
    private readonly IOptions<Settings> _settings;
    private readonly IAlgorithm _algorithm;
    private readonly List<IDirectedGraph> _directedGraphs;
    private readonly IHistogram _histogram;
    private readonly IMetadata _metadata;

    public Process(IOptions<Settings> settings,
                   IAlgorithm algorithm,
                   IEnumerable<IDirectedGraph> directedGraphs,
                   IHistogram histogram,
                   IMetadata metadata)
    { 
        _settings = settings;
        _algorithm = algorithm;
        _directedGraphs = directedGraphs.ToList();
        _histogram = histogram;
        _metadata = metadata;
    }

    public void Run()
	{
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        ConsoleOutput.WriteAsciiArtLogo();
        ConsoleOutput.WriteSettings(_settings.Value);

        ConsoleOutput.WriteHeading("Series data");

        List<int> inputValues = GenerateInputValues(stopwatch);

        ConsoleOutput.WriteHeading("Algorithm execution");

        Console.Write($"Running 3x+1 algorithm on {inputValues.Count} numbers... ");

        List<List<int>> outputValues = _algorithm.Run(inputValues);

        ConsoleOutput.WriteDone();

        IDirectedGraph graph = _directedGraphs.Where(graph => graph.Dimensions == _settings.Value.ParsedGraphDimensions)
                                              .First();
        
        foreach (List<int> values in outputValues)
        {
            graph.AddSeries(values);
        }

        ConsoleOutput.WriteHeading("Directed graph");

        graph.PositionNodes();
        graph.DrawGraph();

        _histogram.GenerateHistogram(outputValues);
        _metadata.GenerateMedatadataFile(outputValues);

        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;

        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           ts.Minutes, ts.Seconds, ts.Milliseconds);

        ConsoleOutput.WriteHeading($"Process completed. Execution time: {elapsedTime}");
    }

    private List<int> GenerateInputValues(Stopwatch stopwatch)
    {
        var random = new Random();
        var inputValues = new List<int>();

        if (string.IsNullOrEmpty(_settings.Value.UseTheseNumbers))
        {
            Console.Write($"Generating {_settings.Value.NumberOfSeries} random numbers from 1 to {_settings.Value.MaxStartingNumber}... ");

            while (inputValues.Count < _settings.Value.NumberOfSeries)
            {
                if (stopwatch.Elapsed.TotalSeconds >= 10)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Gave up generating {_settings.Value.NumberOfSeries} random numbers. Generated {inputValues.Count}");
                    Console.WriteLine();

                    break;
                }

                int randomValue = random.Next(0, _settings.Value.MaxStartingNumber) + 1;

                if (_settings.Value.ListOfNumbersToExclude.Contains(randomValue))
                {
                    continue;
                }

                if (!inputValues.Contains(randomValue))
                {
                    inputValues.Add(randomValue);
                }
            }

            //populate the property as the number list is used to generate a hash value for the directory name
            _settings.Value.UseTheseNumbers = string.Join(", ", inputValues);

            ConsoleOutput.WriteDone();
        }
        else
        {
            inputValues = _settings.Value.ListOfSeriesNumbers;
        }

        return inputValues;
    }
}