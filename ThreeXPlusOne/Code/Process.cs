using System.Diagnostics;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class Process
{
	public static void Run(Settings settings)
	{
        var random = new Random();
        var inputValues = new List<int>();

        if (string.IsNullOrEmpty(settings.UseOnlyTheseNumbers))
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            while (inputValues.Count < settings.NumberOfSeries)
            {
                if (stopwatch.Elapsed.TotalSeconds >= 10)
                {
                    stopwatch.Stop();

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
        }
        else
        {
            inputValues = settings.ListOfManualSeriesNumbers;
        }
        
        List<List<int>> outputValues = Algorithm.Run(inputValues);

        var graph = new DirectedGraph(settings);

        Console.ForegroundColor = ConsoleColor.White;

        ConsoleOutput.WriteAsciiArtLogo();

        ConsoleOutput.WriteHeading("Settings");

        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null).ToList();

        foreach(var property in settingsProperties)
        {
            var value = property.GetValue(settings, null);

            Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write($"    {property.Name}: ");

            Console.ForegroundColor = ConsoleColor.White;

            Console.Write($"{value}");
            Console.WriteLine();
        }

        ConsoleOutput.WriteSeparator();

        if (string.IsNullOrEmpty(settings.UseOnlyTheseNumbers) &&
            settings.NumberOfSeries > outputValues.Count)
        {
            Console.WriteLine($"Gave up generating {settings.NumberOfSeries} random numbers. Generated {inputValues.Count}");
            Console.WriteLine();
        }

        ConsoleOutput.WriteHeading("Adding series for the following numbers:");

        var lcv = 1;

        foreach (List<int> values in outputValues)
        {
            Console.Write($"{values[0]}, ");

            if (lcv % 20 == 0)
            {
                Console.WriteLine();
            }

            graph.AddSeries(values);

            lcv++;
        }

        Console.Write("... ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Done");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();

        ConsoleOutput.WriteSeparator();

        ConsoleOutput.WriteHeading("Top 10 longest series:");

        foreach ((int FirstNumber, int Count) in GenerateTop10Series(outputValues))
        {
            Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write($"    {FirstNumber}: ");

            Console.ForegroundColor = ConsoleColor.White;

            Console.Write($"{Count} in series");

            Console.WriteLine();
        }

        ConsoleOutput.WriteSeparator();

        ConsoleOutput.WriteHeading("Graph generation:");

        graph.PositionNodes();
        graph.Draw(settings);

        Histogram.GenerateHistogram(outputValues, settings);

        ConsoleOutput.WriteSeparator();

        Console.WriteLine("Process complete.");
        Console.WriteLine();
    }

    private static List<(int FirstNumber, int Count)> GenerateTop10Series(List<List<int>> series)
    {
        return series.Where(list => list.Any()).Select(list => (list.First(), list.Count))
                                               .OrderByDescending(item => item.Count)
                                               .Take(10)
                                               .ToList();
    }
}