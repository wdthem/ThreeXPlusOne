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
}