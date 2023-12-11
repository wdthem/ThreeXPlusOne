using System;
using System.Diagnostics;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class Process
{
    public static void Run(Settings settings)
	{
        var stopwatch = new Stopwatch();

        stopwatch.Start();

        ConsoleOutput.WriteAsciiArtLogo();
        ConsoleOutput.WriteSettings(settings);

        ConsoleOutput.WriteHeading("Series data");

        List<int> inputValues = GenerateInputValues(settings, stopwatch);

        ConsoleOutput.WriteHeading("Algorithm execution");

        Console.Write($"Running 3x+1 algorithm on {inputValues.Count} numbers... ");

        List<List<int>> outputValues = Algorithm.Run(inputValues);

        ConsoleOutput.WriteDone();

        var graph = new DirectedGraph(settings);
        
        foreach (List<int> values in outputValues)
        {
            graph.AddSeries(values);
        }

        ConsoleOutput.WriteHeading("Directed graph");

        graph.PositionNodes();
        graph.Draw(settings);

        Histogram.GenerateHistogram(outputValues, settings);
        Metadata.GenerateMedatadataFile(settings, outputValues);

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