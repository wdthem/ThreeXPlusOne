using SkiaSharp;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public abstract class DirectedGraph
{
    protected readonly Dictionary<int, DirectedGraphNode> _nodes;
    protected readonly Random _random;

    public DirectedGraph()
	{
        _nodes = new Dictionary<int, DirectedGraphNode>();
        _random = new Random();
    }

    public void AddSeries(List<int> series)
    {
        DirectedGraphNode? previousNode = null;
        int currentDepth = series.Count;

        foreach (var number in series)
        {
            if (!_nodes.TryGetValue(number, out DirectedGraphNode? currentNode))
            {
                currentNode = new DirectedGraphNode(number)
                {
                    Depth = currentDepth // Set the initial depth for the node
                };

                _nodes.Add(number, currentNode);
            }

            // Check if this is a deeper path to the current node
            if (currentDepth < currentNode.Depth)
            {
                currentNode.Depth = currentDepth;
            }

            if (previousNode != null)
            {
                previousNode.Parent = currentNode;

                // Check if previousNode is already a child to prevent duplicate additions
                if (!currentNode.Children.Contains(previousNode))
                {
                    currentNode.Children.Add(previousNode);
                }
            }

            previousNode = currentNode;

            currentDepth--;  // decrement the depth as we move through the series
        }
    }

    protected static void SaveCanvas(SKSurface surface, string path)
    {
        Console.WriteLine();
        Console.Write("Saving image... ");

        using (var image = surface.Snapshot())
        using (var data = image.Encode(SKEncodedImageFormat.Png, 25))
        using (var stream = File.OpenWrite(path))
        {
            data.SaveTo(stream);
        }

        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Saved to: {path}");
        Console.ForegroundColor = ConsoleColor.White;
    }

    protected SKColor GetRandomColor()
    {
        byte red, green, blue;

        do
        {
            red = (byte)_random.Next(256);
            green = (byte)_random.Next(256);
            blue = (byte)_random.Next(256);
        }
        while (red == 0 && green == 0 && blue == 0); // Repeat if the color is black

        return new SKColor(red, green, blue);
    }

    protected static (double x, double y) RotatePointAntiClockWise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x - sinTheta * y;
        double yNew = sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }

    protected static (double x, double y) RotatePointClockwise(double x, double y, double angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x + sinTheta * y;
        double yNew = -sinTheta * x + cosTheta * y;

        return (xNew, yNew);
    }
}