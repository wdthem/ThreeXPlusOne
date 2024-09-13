using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public abstract partial class DirectedGraph
{
    /// <summary>
    /// Nested class to encapsulate all shared methods that manipulate node position.
    /// </summary>
    protected class NodePositions()
    {
        /// <summary>
        /// The graph starts out at 0,0 with 0 width and 0 height. This means that nodes go into negative space as they are initially positioned, 
        /// so all coordinates need to be shifted to make sure all are in positive space.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="xNodeSpacer"></param>
        /// <param name="yNodeSpacer"></param>
        /// <param name="nodeRadius"></param>
        public static void TranslateNodesToPositiveCoordinates(Dictionary<int, DirectedGraphNode> nodes,
                                                               double xNodeSpacer,
                                                               double yNodeSpacer,
                                                               double nodeRadius)
        {
            double minX = nodes.Values.Min(node => node.Position.X);
            double minY = nodes.Values.Min(node => node.Position.Y);

            // Calculate the translation needed to move all nodes to positive coordinates with padding
            double translationX = -minX + xNodeSpacer + nodeRadius;
            double translationY = -minY + yNodeSpacer + nodeRadius;

            foreach (DirectedGraphNode node in nodes.Values)
            {
                node.Position = (node.Position.X + translationX, node.Position.Y + translationY);

                if (node.SpiralCenter != null)
                {
                    node.SpiralCenter = (node.SpiralCenter.Value.X + translationX, node.SpiralCenter.Value.Y + translationY);
                }
            }
        }

        /// <summary>
        /// Rotate a node's x,y coordinate position based on whether the node's integer value is even or odd.
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