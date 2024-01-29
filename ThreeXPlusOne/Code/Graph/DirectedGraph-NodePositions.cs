using ThreeXPlusOne.Code.Interfaces.Helpers;
using ThreeXPlusOne.Code.Models;

namespace ThreeXPlusOne.Code.Graph;

public abstract partial class DirectedGraph
{
    /// <summary>
    /// Nested class to encapsulate all shared methods that manipulate node position
    /// </summary>
    protected class NodePositions(IConsoleHelper consoleHelper)
    {
        private readonly Dictionary<(int, int), List<(double X, double Y)>> _nodeGrid = [];

        /// <summary>
        /// The graph starts out at 0,0 with 0 width and 0 height. This means that nodes go into negative space as they are initially positioned, 
        /// so all coordinates need to be shifted to make sure all are in positive space
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="xNodeSpacer"></param>
        /// <param name="yNodeSpacer"></param>
        /// <param name="nodeRadius"></param>
        public void MoveNodesToPositiveCoordinates(Dictionary<int, DirectedGraphNode> nodes,
                                                   double xNodeSpacer,
                                                   double yNodeSpacer,
                                                   double nodeRadius)
        {
            consoleHelper.Write("Adjusting node positions to fit on canvas... ");

            double minX = nodes.Values.Min(node => node.Position.X);
            double minY = nodes.Values.Min(node => node.Position.Y);

            double translationX = minX < 0 ? -minX + xNodeSpacer + nodeRadius : 0;
            double translationY = minY < 0 ? -minY + yNodeSpacer + nodeRadius : 0;

            foreach (DirectedGraphNode node in nodes.Values)
            {
                node.Position = (node.Position.X + translationX,
                                 node.Position.Y + translationY);
            }

            consoleHelper.WriteDone();
        }

        /// <summary>
        /// Determine if the node that was just positioned is too close to neighbouring nodes (and thus overlapping)
        /// </summary>
        /// <param name="newNode"></param>
        /// <param name="minDistance"></param>
        /// <returns></returns>
        public bool NodeOverlapsNeighbours(DirectedGraphNode newNode,
                                           double minDistance)
        {
            (int, int) cell = GetGridCellForNode(newNode, minDistance);

            // Check this cell and adjacent cells
            foreach ((int, int) offset in new[] { (0, 0), (1, 0), (0, 1), (-1, 0), (0, -1) })
            {
                (int, int) checkCell = (cell.Item1 + offset.Item1,
                                        cell.Item2 + offset.Item2);

                if (_nodeGrid.TryGetValue(checkCell, out var nodesInCell))
                {
                    foreach ((double X, double Y) node in nodesInCell)
                    {
                        if (Distance(newNode.Position, node) < minDistance)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Add the node to the grid dictionary to keep track of node positions via a grid system
        /// </summary>
        /// <param name="node"></param>
        /// <param name="minDistance"></param>
        public void AddNodeToGrid(DirectedGraphNode node,
                                  double minDistance)
        {
            (int, int) cell = GetGridCellForNode(node, minDistance);

            if (!_nodeGrid.TryGetValue(cell, out List<(double X, double Y)>? value))
            {
                value = ([]);
                _nodeGrid[cell] = value;
            }

            value.Add(node.Position);
        }

        /// <summary>
        /// Retrieve the cell in the grid object in which the node is positioned
        /// </summary>
        /// <param name="node"></param>
        /// <param name="cellSize"></param>
        /// <returns></returns>
        private static (int, int) GetGridCellForNode(DirectedGraphNode node,
                                                     double cellSize)
        {
            return ((int)(node.Position.X / cellSize), (int)(node.Position.Y / cellSize));
        }
    }
}