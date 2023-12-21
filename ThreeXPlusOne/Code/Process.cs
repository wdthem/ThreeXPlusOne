using System.Diagnostics;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Process(IOptions<Settings> settings,
                     IAlgorithm algorithm,
                     IEnumerable<IDirectedGraph> directedGraphs,
                     IHistogram histogram,
                     IMetadata metadata) : IProcess
{
    /// <summary>
    /// Run the algorithm and data generation based on the user-provided settings
    /// </summary>
    public void Run()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        ConsoleOutput.WriteAsciiArtLogo();
        ConsoleOutput.WriteSettings(settings.Value);

        ConsoleOutput.WriteHeading("Series data");

        List<int> inputValues = GenerateInputValues(stopwatch);

        ConsoleOutput.WriteHeading("Algorithm execution");

        Console.Write($"Running 3x+1 algorithm on {inputValues.Count} numbers... ");

        List<List<int>> seriesData = algorithm.Run(inputValues);

        ConsoleOutput.WriteDone();

        IDirectedGraph graph = directedGraphs.ToList()
                                             .Where(graph => graph.Dimensions == settings.Value.SanitizedGraphDimensions)
                                             .First();

        foreach (List<int> series in seriesData)
        {
            graph.AddSeries(series);
        }

        ConsoleOutput.WriteHeading("Directed graph");

        graph.PositionNodes();
        graph.DrawGraph();

        histogram.GenerateHistogram(seriesData);
        metadata.GenerateMedatadataFile(seriesData);

        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;

        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           ts.Minutes, ts.Seconds, ts.Milliseconds);

        ConsoleOutput.WriteHeading($"Process completed. Execution time: {elapsedTime}");
    }

    /// <summary>
    /// Get the list of numbers to use to run through the algorithm
    /// Either:
    ///     Random numbers - the amount specified in settings; or
    ///     The list specified by the user in settings (this takes priority)
    /// </summary>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    private List<int> GenerateInputValues(Stopwatch stopwatch)
    {
        var random = new Random();
        var inputValues = new List<int>();

        if (string.IsNullOrWhiteSpace(settings.Value.UseTheseNumbers))
        {
            Console.Write($"Generating {settings.Value.NumberOfSeries} random numbers from 1 to {settings.Value.MaxStartingNumber}... ");

            while (inputValues.Count < settings.Value.NumberOfSeries)
            {
                if (stopwatch.Elapsed.TotalSeconds >= 10)
                {
                    if (inputValues.Count == 0)
                    {
                        throw new Exception($"No numbers generated on which to run the algorithm. Check {nameof(settings.Value.ExcludeTheseNumbers)}");
                    }

                    Console.WriteLine();
                    Console.WriteLine($"Gave up generating {settings.Value.NumberOfSeries} random numbers. Generated {inputValues.Count}");
                    Console.WriteLine();

                    break;
                }

                int randomValue = random.Next(0, settings.Value.MaxStartingNumber) + 1;

                if (settings.Value.ListOfNumbersToExclude.Contains(randomValue))
                {
                    continue;
                }

                if (!inputValues.Contains(randomValue))
                {
                    inputValues.Add(randomValue);
                }
            }

            //populate the property as the number list is used to generate a hash value for the directory name
            settings.Value.UseTheseNumbers = string.Join(", ", inputValues);

            ConsoleOutput.WriteDone();
        }
        else
        {
            inputValues = settings.Value.ListOfSeriesNumbers;

            inputValues.RemoveAll(settings.Value.ListOfNumbersToExclude.Contains);

            if (inputValues.Count == 0)
            {
                throw new Exception("No numbers provided on which to run the algorithm");
            }

            Console.WriteLine($"Using series numbers defined in {nameof(settings.Value.UseTheseNumbers)} apart from any excluded in {nameof(settings.Value.ExcludeTheseNumbers)}");
            Console.WriteLine();
        }

        return inputValues;
    }
}