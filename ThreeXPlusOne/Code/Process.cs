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
        Console.WriteLine("3x + 1 Visualizer");
        Console.WriteLine("-----------------");
        Console.WriteLine();
        Console.WriteLine($"Canvas dimensions: {settings.CanvasWidth}W x {settings.CanvasHeight}H");
        Console.WriteLine($"Horizontal space between nodes: {settings.XNodeSpacer}");
        Console.WriteLine($"Vertical space between nodes: {settings.YNodeSpacer}");
        Console.WriteLine($"Rotation angle: {settings.RotationAngle}");
        Console.WriteLine();
        Console.WriteLine("-----------------");
        Console.WriteLine();

        foreach (List<int> values in outputValues)
        {
            graph.AddSeries(values);

            Console.WriteLine($"Added series starting with: {values[0]}");
        }

        Console.WriteLine();

        graph.PositionNodes();

        var outFileType = settings.RotationAngle == 0 ? "NoRotation" : "Rotation";
        var outputFileName = $"/Users/williamthem/Documents/Projects/ThreeXPlusOne/ThreeXPlusOne-" +
                             $"{outFileType}-{Guid.NewGuid()}.png";

        graph.Draw(settings.SaveOutput, outputFileName);

        Console.WriteLine("Press any key to close...");
        Console.ReadKey();
    }
}