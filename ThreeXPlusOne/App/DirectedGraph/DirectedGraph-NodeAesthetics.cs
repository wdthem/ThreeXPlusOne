using System.Drawing;
using System.Text.RegularExpressions;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public abstract partial class DirectedGraph
{
    /// <summary>
    /// Nested class to encapsulate all shared methods that manipulate node colour, shape and orientation
    /// </summary>
    protected partial class NodeAesthetics(IShapeFactory shapeFactory)
    {
        [GeneratedRegex("^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$")]
        private static partial Regex HexCodeRegEx();
        private List<Color> _nodeColors = [];
        private bool _parsedHexCodes = false;

        /// <summary>
        /// Assign a ShapeType to the node and vertices if applicable
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeRadius"></param>
        /// <param name="includePolygonsAsNodes"></param>
        public void SetNodeShape(DirectedGraphNode node,
                                 double nodeRadius,
                                 bool includePolygonsAsNodes)
        {
            double radius = node.Shape.Radius;

            if (radius == 0)
            {
                radius = nodeRadius;
            }

            IShape shape = includePolygonsAsNodes
                                    ? shapeFactory.CreateShape()
                                    : shapeFactory.CreateShape(ShapeType.Ellipse);

            shape.Radius = radius;
            node.Shape = shape;

            node.Shape.SetShapeConfiguration(node);
        }

        /// <summary>
        /// Generate a random colour for the node
        /// </summary>
        /// <returns></returns>
        /// <param name="nodeColors"></param>
        public Color GenerateNodeColor(string nodeColors)
        {
            if (string.IsNullOrWhiteSpace(nodeColors))
            {
                return GenerateRandomNodeColor();
            }

            if (!_parsedHexCodes)
            {
                _nodeColors = GetColorsFromHexCodes(nodeColors);
                _parsedHexCodes = true;
            }

            if (_nodeColors.Count == 0)
            {
                return GenerateRandomNodeColor();
            }

            Color nodeColor = _nodeColors.Count == 1
                                    ? _nodeColors[0]
                                    : _nodeColors[Random.Shared.Next(0, _nodeColors.Count)];

            return nodeColor;
        }

        /// <summary>
        /// Generate a colour for the node's border based on the node's colour
        /// </summary>
        /// <param name="nodeColor"></param>
        /// <returns></returns>
        public static Color GenerateNodeBorderColor(Color nodeColor)
        {
            return AdjustColorBrightness(nodeColor, 1.75f);
        }

        /// <summary>
        /// If a light source is in place, it should impact the colour of nodes.
        /// The closer to the source, the more the impact.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeBaseColor"></param>
        /// <param name="nodeBorderBaseColor"></param>
        /// <param name="lightSourceCoordinates"></param>
        /// <param name="lightSourceMaxDistanceEffect"></param>
        /// <param name="lightSourceColor"></param>
        /// <returns></returns>
        public static void ApplyLightSourceToNode(DirectedGraphNode node,
                                                  Color nodeBaseColor,
                                                  Color nodeBorderBaseColor,
                                                  (double X, double Y) lightSourceCoordinates,
                                                  double lightSourceMaxDistanceEffect,
                                                  Color lightSourceColor)
        {
            Color nodeColor;
            Color nodeBorderColor;
            double distance = Distance((node.Position.X, node.Position.Y),
                                       (lightSourceCoordinates.X, lightSourceCoordinates.Y));

            double lightIntensity = 0.4f; // Adjust this value between 0 and 1 to control the light's power

            if (distance < lightSourceMaxDistanceEffect)
            {
                double normalizedDistance = distance / lightSourceMaxDistanceEffect;
                double smoothFactor = 1 - Math.Pow(normalizedDistance, 2); // Quadratic decay

                double blendFactor = smoothFactor * lightIntensity;
                blendFactor = Math.Clamp(blendFactor, 0, 1); // Ensure it's within bounds

                nodeColor = BlendColor(nodeBaseColor, lightSourceColor, blendFactor);
                nodeBorderColor = BlendColor(nodeBorderBaseColor, lightSourceColor, blendFactor);
            }
            else
            {
                nodeColor = nodeBaseColor;
                nodeBorderColor = nodeBorderBaseColor;
            }

            node.Shape.Color = Color.FromArgb(nodeBaseColor.A, nodeColor.R, nodeColor.G, nodeColor.B);
            node.Shape.BorderColor = Color.FromArgb(nodeBorderBaseColor.A, nodeBorderColor.R, nodeBorderColor.G, nodeBorderColor.B);

            double haloRadius = node.Shape.Radius * 2;
            double intensity = Math.Max(0, 1 - (distance / lightSourceMaxDistanceEffect));
            Color haloColor = Color.FromArgb((byte)(intensity * lightSourceColor.A),
                                             lightSourceColor.R,
                                             lightSourceColor.G,
                                             lightSourceColor.B);

            node.Shape.HaloConfig = (haloRadius, haloColor);
        }

        /// <summary>
        /// Generate a random colour for the node
        /// </summary>
        /// <returns></returns>
        private static Color GenerateRandomNodeColor()
        {
            byte alpha = (byte)Random.Shared.Next(30, 231); //avoid too transparent, and avoid fully opaque
            byte red = (byte)Random.Shared.Next(1, 256);    //for rgb, skip 0 to avoid black
            byte green = (byte)Random.Shared.Next(1, 256);
            byte blue = (byte)Random.Shared.Next(1, 256);

            return Color.FromArgb(alpha, red, green, blue);
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
        /// Get valid hex codes passed in as app settings
        /// </summary>
        /// <param name="hexCodes"></param>
        /// <returns></returns>
        private static List<Color> GetColorsFromHexCodes(string hexCodes)
        {
            List<Color> colors = [];
            List<string> rawCodes = [.. hexCodes.Split(",")];
            Regex hexCodeRegex = HexCodeRegEx();

            foreach (string rawCode in rawCodes)
            {
                if (!hexCodeRegex.IsMatch(rawCode))
                {
                    continue;
                }

                Color colorFromHexCode = ColorTranslator.FromHtml(rawCode);

                if (colorFromHexCode == Color.Empty)
                {
                    continue;
                }

                Color colorWithAlpha = Color.FromArgb((byte)Random.Shared.Next(30, 231), //avoid too transparent, and avoid fully opaque
                                                      colorFromHexCode.R,
                                                      colorFromHexCode.G,
                                                      colorFromHexCode.B);

                colors.Add(colorWithAlpha);

                colors.AddRange(GenerateLighterAndDarkerColors(colorWithAlpha));
            }

            return colors;
        }

        /// <summary>
        /// For user-supplied colours, get colors slightly lighter and darker than the supplied colour to add
        /// dynamism
        /// </summary>
        /// <param name="baseColor"></param>
        /// <returns></returns>
        private static List<Color> GenerateLighterAndDarkerColors(Color baseColor)
        {
            List<Color> colors = [];

            // Lighter colors
            colors.Add(AdjustColorBrightness(baseColor, 1.1f));
            colors.Add(AdjustColorBrightness(baseColor, 1.2f));

            // Darker colors
            colors.Add(AdjustColorBrightness(baseColor, 0.9f));
            colors.Add(AdjustColorBrightness(baseColor, 0.8f));

            return colors;
        }

        /// <summary>
        /// Adjust the brightness of the given color to geta  slightly different shade
        /// </summary>
        /// <param name="color"></param>
        /// <param name="factor"></param>
        /// <returns></returns>
        private static Color AdjustColorBrightness(Color color, float factor)
        {
            int r = (int)Math.Clamp(color.R * factor, 0, 255);
            int g = (int)Math.Clamp(color.G * factor, 0, 255);
            int b = (int)Math.Clamp(color.B * factor, 0, 255);

            return Color.FromArgb(color.A, r, g, b);
        }
    }
}
