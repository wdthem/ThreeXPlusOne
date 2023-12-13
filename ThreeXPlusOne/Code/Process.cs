using System.Diagnostics;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Process : IProcess
{
    private readonly IOptions<Settings> _settings;
    private readonly List<IDirectedGraph> _directedGraphs;

    public Process(IOptions<Settings> settings,
                   IEnumerable<IDirectedGraph> directedGraphs)
    {
        _settings = settings;
        _directedGraphs = directedGraphs.ToList();
    }

    public void Run()
	{
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        ConsoleOutput.WriteAsciiArtLogo();
        ConsoleOutput.WriteSettings(_settings.Value);

        ConsoleOutput.WriteHeading("Series data");

        List<int> inputValues = GenerateInputValues(_settings.Value, stopwatch);

        ConsoleOutput.WriteHeading("Algorithm execution");

        Console.Write($"Running 3x+1 algorithm on {inputValues.Count} numbers... ");

        List<List<int>> outputValues = Algorithm.Run(inputValues);

        ConsoleOutput.WriteDone();

        IDirectedGraph graph;

        if (_settings.Value.ParsedGraphDimensions == 2)
        {
            graph = _directedGraphs[0];
        }
        else
        {
            graph = _directedGraphs[1];
        }
        
        foreach (List<int> values in outputValues)
        {
            graph.AddSeries(values);
        }

        ConsoleOutput.WriteHeading("Directed graph");

        graph.PositionNodes();
        graph.Draw(_settings.Value);

        Histogram.GenerateHistogram(outputValues, _settings.Value);
        Metadata.GenerateMedatadataFile(_settings.Value, outputValues);

        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;

        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           ts.Minutes, ts.Seconds, ts.Milliseconds);

        ConsoleOutput.WriteHeading($"Process completed. Execution time: {elapsedTime}");
    }

    private static List<int> GenerateInputValues(Settings settings, Stopwatch stopwatch)
    {
        var random = new Random();
        var inputValues = new List<int>();

        if (string.IsNullOrEmpty(settings.UseOnlyTheseNumbers))
        {
            Console.Write($"Generating {settings.NumberOfSeries} random numbers from 1 to {settings.MaxStartingNumber}... ");

            while (inputValues.Count < settings.NumberOfSeries)
            {
                if (stopwatch.Elapsed.TotalSeconds >= 10)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Gave up generating {settings.NumberOfSeries} random numbers. Generated {inputValues.Count}");
                    Console.WriteLine();

                    break;
                }

                int randomValue = random.Next(0, settings.MaxStartingNumber) + 1;

                if (settings.ListOfNumbersToExclude.Contains(randomValue))
                {
                    continue;
                }

                if (!inputValues.Contains(randomValue))
                {
                    inputValues.Add(randomValue);
                }
            }

            ConsoleOutput.WriteDone();
        }
        else
        {
            inputValues = settings.ListOfManualSeriesNumbers;
        }

        return inputValues;
    }
}