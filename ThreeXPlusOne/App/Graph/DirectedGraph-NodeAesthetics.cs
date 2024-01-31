using System.Drawing;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Graph;

public abstract partial class DirectedGraph
{
    /// <summary>
    /// Nested class to encapsulate all shared methods that manipulate node colour, shape and orientation
    /// </summary>
    protected class NodeAesthetics()
    {
        /// <summary>
        /// Assign a ShapeType to the node and vertices if applicable
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeRadius"></param>
        /// <param name="includePolygonsAsNodes"></param>
        public static void SetNodeShape(DirectedGraphNode node,
                                        double nodeRadius,
                                        bool includePolygonsAsNodes)
        {
            if (node.Shape.Radius == 0)
            {
                node.Shape.Radius = nodeRadius;
            }

            int numberOfSides = Random.Shared.Next(0, 11);

            if (!includePolygonsAsNodes || numberOfSides == 0)
            {
                node.Shape.ShapeType = ShapeType.Circle;

                return;
            }

            if (numberOfSides == 1 || numberOfSides == 2)
            {
                numberOfSides = Random.Shared.Next(3, 11); //cannot have 1 or 2 sides, so re-select
            }

            node.Shape.ShapeType = ShapeType.Polygon;

            double rotationAngle = Random.Shared.NextDouble() * 2 * Math.PI;

            for (int i = 0; i < numberOfSides; i++)
            {
                double angle = (2 * Math.PI / numberOfSides * i) + rotationAngle;

                node.Shape.PolygonVertices.Add((node.Position.X + node.Shape.Radius * Math.Cos(angle),
                                                node.Position.Y + node.Shape.Radius * Math.Sin(angle)));
            }
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
        /// Generate a random colour for the node
        /// </summary>
        /// <returns></returns>
        public static Color GenerateNodeColor()
        {
            byte alpha = (byte)Random.Shared.Next(30, 231); //avoid too transparent, and avoid fully opaque
            byte red = (byte)Random.Shared.Next(1, 256);    //for rgb, skip 0 to avoid black
            byte green = (byte)Random.Shared.Next(1, 256);
            byte blue = (byte)Random.Shared.Next(1, 256);

            return Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// If a light source is in place, it should impact the colour of nodes.
        /// The closer to the source, the more the impact.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeBaseColor"></param>
        /// <param name="lightSourceCoordinates"></param>
        /// <param name="lightSourceMaxDistanceEffect"></param>
        /// <param name="lightSourceColor"></param>
        /// <returns></returns>
        public static void ApplyLightSourceToNode(DirectedGraphNode node,
                                                  Color nodeBaseColor,
                                                  (double X, double Y) lightSourceCoordinates,
                                                  double lightSourceMaxDistanceEffect,
                                                  Color lightSourceColor)
        {
            Color nodeColor;
            double distance = Distance((node.Position.X, node.Position.Y),
                                       (lightSourceCoordinates.X, lightSourceCoordinates.Y));

            double additionalOpacityFactor;

            double lightIntensity = 0.4f; // Adjust this value between 0 and 1 to control the light's power

            if (distance < lightSourceMaxDistanceEffect)
            {
                additionalOpacityFactor = distance / lightSourceMaxDistanceEffect;
                additionalOpacityFactor = Math.Clamp(additionalOpacityFactor, 0, 1);

                // Apply the light intensity to the blend factor
                double blendFactor = additionalOpacityFactor * lightIntensity;
                nodeColor = BlendColor(nodeBaseColor, lightSourceColor, 1 - blendFactor);
            }
            else
            {
                //else leave opacity at the randomly select value
                nodeColor = nodeBaseColor;
                additionalOpacityFactor = 1.0f;
            }

            byte finalAlpha = (byte)(nodeBaseColor.A * additionalOpacityFactor);

            node.Shape.Color = Color.FromArgb(finalAlpha, nodeColor.R, nodeColor.G, nodeColor.B);

            double haloRadius = node.Shape.Radius * 2;
            double intensity = Math.Max(0, 1 - (distance / lightSourceMaxDistanceEffect));
            Color haloColor = Color.FromArgb((byte)(intensity * lightSourceColor.A),
                                             lightSourceColor.R,
                                             lightSourceColor.G,
                                             lightSourceColor.B);

            node.Shape.HaloConfig = (haloRadius, haloColor);
        }

        /// <summary>
        /// Blend the node's colour with the light source, adjusted for distance from the light source
        /// </summary>
        /// <param name="baseColor"></param>
        /// <param name="blendColor"></param>
        /// <param name="blendFactor"></param>
        /// <returns></returns>
        private static Color BlendColor(Color baseColor,
                                        Color blendColor,
                                        double blendFactor)
        {
            byte r = (byte)((baseColor.R * (1 - blendFactor)) + (blendColor.R * blendFactor));
            byte g = (byte)((baseColor.G * (1 - blendFactor)) + (blendColor.G * blendFactor));
            byte b = (byte)((baseColor.B * (1 - blendFactor)) + (blendColor.B * blendFactor));

            return Color.FromArgb(255, r, g, b);
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