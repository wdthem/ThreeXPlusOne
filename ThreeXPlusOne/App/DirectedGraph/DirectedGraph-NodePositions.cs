using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public abstract partial class DirectedGraph
{
    /// <summary>
    /// Nested class to encapsulate all shared methods that manipulate node position
    /// </summary>
    protected class NodePositions(IConsoleService consoleService)
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
            consoleService.Write("Adjusting node positions to fit on canvas... ");

            double minX = nodes.Values.Min(node => node.Position.X);
            double minY = nodes.Values.Min(node => node.Position.Y);

            double translationX = minX < 0 ? -minX + xNodeSpacer + nodeRadius : 0;
            double translationY = minY < 0 ? -minY + yNodeSpacer + nodeRadius : 0;

            foreach (DirectedGraphNode node in nodes.Values)
            {
                node.Position = (node.Position.X + translationX,
                                 node.Position.Y + translationY);
            }

            consoleService.WriteDone();
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
        /// Rotate a node's x,y coordinate position based on whether the node's integer value is even or odd
        /// If even, rotate clockwise. If odd, rotate anti-clockwise. But if the coordinates are in negative space, reverse this.
        /// </summary>
        /// <param name="nodeValue"></param>
        /// <param name="rotationAngle"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static (double X, double Y) RotateNode(int nodeValue,
                                                      double rotationAngle,
                                                      double x,
                                                      double y)
        {
            (double X, double Y) rotatedPosition;

            // Check if either coordinate is negative to know how to rotate
            bool isInNegativeSpace = x < 0 || y < 0;

            if ((nodeValue % 2 == 0 && !isInNegativeSpace) || (nodeValue % 2 != 0 && isInNegativeSpace))
            {
                rotatedPosition = RotatePointClockwise(x, y, rotationAngle);
            }
            else
            {
                rotatedPosition = RotatePointAntiClockwise(x, y, rotationAngle);
            }

            return rotatedPosition;
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


        /// <summary>
        /// Rotate the node's position clockwise based on the angle provided by the user. This gives a more artistic feel to the generated graph.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="angleDegrees"></param>
        /// <returns></returns>
        private static (double x, double y) RotatePointClockwise(double x,
                                                                 double y,
                                                                 double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);

            double xNew = cosTheta * x + sinTheta * y;
            double yNew = -sinTheta * x + cosTheta * y;

            return (xNew, yNew);
        }

        /// <summary>
        /// Rotate the node's position anti-clockwise based on the angle provided by the user. This gives a more artistic feel to the generated graph.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="angleDegrees"></param>
        /// <returns></returns>
        private static (double x, double y) RotatePointAntiClockwise(double x,
                                                                     double y,
                                                                     double angleDegrees)
        {
            double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

            double cosTheta = Math.Cos(angleRadians);
            double sinTheta = Math.Sin(angleRadians);

            double xNew = cosTheta * x - sinTheta * y;
            double yNew = sinTheta * x + cosTheta * y;

            return (xNew, yNew);
        }
    }
}