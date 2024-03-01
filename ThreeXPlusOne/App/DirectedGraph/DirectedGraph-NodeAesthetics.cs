using System.Drawing;
using System.Text.RegularExpressions;
using ThreeXPlusOne.App.DirectedGraph.Shapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public abstract partial class DirectedGraph
{
    /// <summary>
    /// Nested class to encapsulate all shared methods that manipulate node colour, shape and orientation
    /// </summary>
    protected partial class NodeAesthetics(ShapeFactory shapeFactory)
    {
        [GeneratedRegex("^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$")]
        private static partial Regex HexCodeRegEx();
        private readonly HashSet<Color> _seriesColors = [];
        private readonly Dictionary<int, Color> _seriesColorMappings = [];
        private List<Color> _nodeColors = [];
        private List<ShapeType> _nodeShapes = [];
        private bool _parsedHexCodes = false;
        private bool _parsedNodeShapes = false;

        /// <summary>
        /// Assign an IShape object to the node
        /// </summary>
        /// <param name="node"></param>
        /// <param name="nodeRadius"></param>
        /// <param name="nodeShapes"></param>
        public void SetNodeShape(DirectedGraphNode node,
                                 double nodeRadius,
                                 string nodeShapes)
        {
            if (!_parsedNodeShapes)
            {
                _nodeShapes = ParseShapeTypes(nodeShapes);

                _parsedNodeShapes = true;
            }

            double radius = node.Shape.Radius;

            if (radius == 0)
            {
                radius = nodeRadius;
            }

            IShape shape = _nodeShapes.Count == 0
                                    ? shapeFactory.CreateShape()
                                    : shapeFactory.CreateShape(_nodeShapes);

            shape.Radius = radius;
            shape.SetShapeConfiguration(node.Position, radius);

            node.Shape = shape;
        }

        /// <summary>
        /// Generate a colour for the node
        /// </summary>
        /// <returns></returns>
        /// <param name="node"></param>
        /// <param name="nodeColors">Exclusive colours for nodes</param>
        /// <param name="nodeColorsBias">Random colours but with some bias toward these</param>
        /// <param name="colorCodeSeries">Each number series has its own random colour</param>
        public void SetNodeColor(DirectedGraphNode node,
                                 string nodeColors,
                                 string nodeColorsBias,
                                 bool colorCodeSeries)
        {
            float borderColorAdjustment = 1.75f;

            if (colorCodeSeries)
            {
                node.Shape.Color = GetNodeSeriesColor(node.SeriesNumber);
                node.Shape.BorderColor = AdjustColorBrightness(node.Shape.Color, borderColorAdjustment);

                return;
            }

            if (string.IsNullOrWhiteSpace(nodeColors))
            {
                if (!_parsedHexCodes && !string.IsNullOrWhiteSpace(nodeColorsBias))
                {
                    _nodeColors = GetColorsFromHexCodes(nodeColorsBias);
                    _parsedHexCodes = true;
                }

                node.Shape.Color = GenerateRandomNodeColor();
                node.Shape.BorderColor = AdjustColorBrightness(node.Shape.Color, borderColorAdjustment);

                return;
            }

            if (!_parsedHexCodes)
            {
                _nodeColors = GetColorsFromHexCodes(nodeColors);
                _parsedHexCodes = true;
            }

            if (_nodeColors.Count == 0)
            {
                node.Shape.Color = GenerateRandomNodeColor();
                node.Shape.BorderColor = AdjustColorBrightness(node.Shape.Color, borderColorAdjustment);

                return;
            }

            node.Shape.Color = _nodeColors.Count == 1
                                                ? _nodeColors[0]
                                                : _nodeColors[Random.Shared.Next(_nodeColors.Count)];

            node.Shape.BorderColor = AdjustColorBrightness(node.Shape.Color, borderColorAdjustment);
        }

        /// <summary>
        /// If a light source is in place, it should impact the colour of nodes.
        /// The closer to the source, the more the impact.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="lightSourceCoordinates"></param>
        /// <param name="maxAffectDistance"></param>
        /// <param name="lightSourceColor"></param>
        /// <returns></returns>
        public static void ApplyLightSourceToNodes(Dictionary<int, DirectedGraphNode> nodes,
                                                   (double X, double Y) lightSourceCoordinates,
                                                   double maxAffectDistance,
                                                   Color lightSourceColor)
        {
            foreach (DirectedGraphNode node in nodes.Values)
            {
                double distance = Distance(node.Position,
                                           lightSourceCoordinates);

                if (distance > maxAffectDistance)
                {
                    continue;
                }

                double lightIntensity = 0.4f; // Adjust this value between 0 and 1 to control the light's power
                double normalizedDistance = distance / maxAffectDistance;
                double smoothFactor = 1 - Math.Pow(normalizedDistance, 2); // Quadratic decay
                double blendFactor = Math.Clamp(smoothFactor * lightIntensity, 0, 1); // Ensure it's within bounds

                node.Shape.Color = BlendColor(node.Shape.Color, lightSourceColor, blendFactor);
                node.Shape.BorderColor = BlendColor(node.Shape.BorderColor, lightSourceColor, blendFactor);

                // Calculate and set the gradient start and end points of the 3D shape sides
                (double X, double Y) lightDirection = (node.Position.X - lightSourceCoordinates.X, node.Position.Y - lightSourceCoordinates.Y);
                double lightDirectionMagnitude = Math.Sqrt(lightDirection.X * lightDirection.X + lightDirection.Y * lightDirection.Y);
                (double X, double Y) normalizedLightDirection = (lightDirection.X / lightDirectionMagnitude, lightDirection.Y / lightDirectionMagnitude);

                node.Shape.ThreeDimensionalSideGradientStartColor = BlendColor(node.Shape.ThreeDimensionalSideGradientStartColor, lightSourceColor, blendFactor);

                node.Shape.SetNodeThreeDimensionalGradientPoints((node.Position.X - normalizedLightDirection.X * node.Shape.Radius,
                                                                  node.Position.Y - normalizedLightDirection.Y * node.Shape.Radius),
                                                                  (node.Position.X + normalizedLightDirection.X * node.Shape.Radius,
                                                                  node.Position.Y + normalizedLightDirection.Y * node.Shape.Radius));
            }
        }

        /// <summary>
        /// Get valid ShapeTypes from the user-provided data
        /// </summary>
        /// <param name="rawShapeTypes"></param>
        /// <returns></returns>
        private static List<ShapeType> ParseShapeTypes(string rawShapeTypes)
        {
            List<ShapeType> shapeTypes = [];
            List<string> rawShapesTypesList = [.. rawShapeTypes.Split(',')];

            foreach (string rawShapeType in rawShapesTypesList)
            {
                if (Enum.TryParse(rawShapeType, out ShapeType shapeType))
                {
                    shapeTypes.Add(shapeType);
                }
            }

            return shapeTypes;
        }

        /// <summary>
        /// Generate a random colour for the node, optionally biased toward selecting user-defined colours
        /// </summary>
        /// <returns></returns>
        private Color GenerateRandomNodeColor()
        {
            double bias = 0.20;

            if (_nodeColors.Count > 0 &&
                Random.Shared.NextDouble() <= bias)
            {
                return _nodeColors[Random.Shared.Next(_nodeColors.Count)];
            }

            byte alpha = (byte)Random.Shared.Next(30, 231); //avoid too transparent, and avoid fully opaque
            byte red = (byte)Random.Shared.Next(1, 256);    //for rgb, skip 0 to avoid black
            byte green = (byte)Random.Shared.Next(1, 256);
            byte blue = (byte)Random.Shared.Next(1, 256);

            return Color.FromArgb(alpha, red, green, blue);
        }

        /// <summary>
        /// Get the random colour assigned to the given number series,
        /// generating the colour if required
        /// </summary>
        /// <param name="seriesNumber"></param>
        /// <returns></returns>
        private Color GetNodeSeriesColor(int seriesNumber)
        {
            if (!_seriesColorMappings.TryGetValue(seriesNumber, out Color seriesColor))
            {
                do
                {
                    seriesColor = GenerateRandomNodeColor();
                } while (_seriesColors.Contains(seriesColor));

                _seriesColorMappings.Add(seriesNumber, seriesColor);
                _seriesColors.Add(seriesColor);
            }

            return seriesColor;
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

            return Color.FromArgb(baseColor.A, r, g, b);
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
            colors.Add(AdjustColorBrightness(baseColor, 1.02f));
            colors.Add(AdjustColorBrightness(baseColor, 1.5f));

            // Darker colors
            colors.Add(AdjustColorBrightness(baseColor, 0.98f));
            colors.Add(AdjustColorBrightness(baseColor, 0.95f));

            return colors;
        }

        /// <summary>
        /// Adjust the brightness of the given color to get a slightly different shade
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
