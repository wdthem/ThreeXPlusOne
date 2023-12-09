using System.Xml.Linq;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public static class Process
{
	public static void Run(Settings settings)
	{
        var random = new Random();
        var inputValues = new List<int>();

        for (int x = 1; x <= settings.NumberOfSeries; x++)
        {
            var randomValue = random.Next(settings.MaxStartingNumber) + 1;

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

        var settingsProperties = typeof(Settings).GetProperties();

        foreach(var property in settingsProperties)
        {
            var value = property.GetValue(settings, null);

            Console.WriteLine($"    {property.Name}: {value}");
        }

        Console.WriteLine();
        Console.WriteLine("-----------------");
        Console.WriteLine();

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

        graph.PositionNodes();
        graph.Draw(settings);

        Console.WriteLine();
        Console.WriteLine("Press any key to quit...");
        Console.ReadKey();
    }
}