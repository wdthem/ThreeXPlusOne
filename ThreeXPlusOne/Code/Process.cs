using System.Diagnostics;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class Process
{
	public static void Run(Settings settings)
	{
        var random = new Random();
        var inputValues = new List<int>();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        while (inputValues.Count < settings.NumberOfSeries)
        {
            if (stopwatch.Elapsed.TotalSeconds >= 10)
            {
                stopwatch.Stop();

                break;
            }

            int randomValue = random.Next(settings.MaxStartingNumber) + 1;

            if (settings.ListOfNumbersToExclude.Contains(randomValue))
            {
                continue;
            }

            if (!inputValues.Contains(randomValue))
            {
                inputValues.Add(randomValue);
            }
        }

        List<List<int>> outputValues = Algorithm.Run(inputValues);

        var graph = new DirectedGraph(settings);

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine("3x + 1 Visualizer");
        Console.WriteLine("-----------------");
        Console.WriteLine();
        Console.WriteLine("Settings:");
        Console.WriteLine();

        var settingsProperties = typeof(Settings).GetProperties().Where(p => p.SetMethod != null).ToList();

        foreach(var property in settingsProperties)
        {
            var value = property.GetValue(settings, null);

            Console.WriteLine($"    {property.Name}: {value}");
        }

        Console.WriteLine();
        Console.WriteLine("-----------------");
        Console.WriteLine();

        if (settings.NumberOfSeries > outputValues.Count)
        {
            Console.WriteLine($"Gave up generating {settings.NumberOfSeries} random numbers. Generated {inputValues.Count}");
            Console.WriteLine();
        }

        Console.Write($"Adding series starting with: ");

        foreach (List<int> values in outputValues)
        {
            Console.Write($"{values[0]}  ");

            graph.AddSeries(values);
        }

        Console.Write("... ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Done");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine();
        Console.WriteLine();

        Console.WriteLine($"Top 10 longest series: ");

        foreach ((int FirstNumber, int Count) in GenerateTop10Series(outputValues))
        {
            Console.WriteLine($"    {FirstNumber}: {Count}");
        }

        Console.WriteLine();
        Console.WriteLine();

        graph.PositionNodes();
        graph.Draw(settings);

        Histogram.GenerateHistogram(outputValues, settings.ImagePath!);

        Console.WriteLine();
        Console.WriteLine("Press any key to quit...");
        Console.ReadKey();
    }

    private static List<(int FirstNumber, int Count)> GenerateTop10Series(List<List<int>> series)
    {
        return series.Where(list => list.Any()).Select(list => (list.First(), list.Count))
                                               .OrderByDescending(item => item.Count)
                                               .Take(10)
                                               .ToList();
    }
}