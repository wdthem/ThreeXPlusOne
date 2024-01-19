using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Code.Models;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code.Graph;

public class ThreeDimensionalDirectedGraph(IOptions<Settings> settings,
                                           IEnumerable<IDirectedGraphService> graphServices,
                                           ILightSourceService lightSourceService,
                                           IConsoleHelper consoleHelper) : DirectedGraph(settings, graphServices, lightSourceService, consoleHelper), IDirectedGraph
{
    private int _nodesPositioned = 0;

    public int Dimensions => 3;

    /// <summary>
    /// Assign sizes to the canvas width and height after having positioned the nodes
    /// </summary>
    public void SetCanvasDimensions()
    {
        SetCanvasSize();
    }

    /// <summary>
    /// Generate a 3D visual representation of the directed graph
    /// </summary>
    public void Draw()
    {
        DrawDirectedGraph();
    }

    /// <summary>
    /// Position the nodes on the graph in pseudo-3D space
    /// </summary>
    public void PositionNodes()
    {
        // Set up the base nodes' positions
        (float X, float Y) base1 = (0, 0);                                          // Node '1' at the bottom
        (float X, float Y) base2 = (0, base1.Y - (_settings.YNodeSpacer * 6));      // Node '2' just above '1'
        (float X, float Y) base4 = (0, base2.Y - (_settings.YNodeSpacer * 5));      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].Position = ApplyPerspectiveTransformToNodePosition(_nodes[1], _settings.DistanceFromViewer);
        _nodes[1].Shape.Radius = 50;
        _nodes[1].IsPositioned = true;

        _nodes[2].Position = base2;
        _nodes[2].Position = ApplyPerspectiveTransformToNodePosition(_nodes[2], _settings.DistanceFromViewer);
        _nodes[2].Shape.Radius = 100;
        _nodes[2].IsPositioned = true;

        _nodes[4].Position = base4;
        _nodes[4].Position = ApplyPerspectiveTransformToNodePosition(_nodes[4], _settings.DistanceFromViewer);
        _nodes[4].Shape.Radius = _settings.NodeRadius;
        _nodes[4].IsPositioned = true;

        List<DirectedGraphNode> nodesToDraw = _nodes.Values.Where(n => n.Depth == _nodes[4].Depth + 1)
                                                           .ToList();

        _nodesPositioned = 3;

        foreach (DirectedGraphNode node in nodesToDraw)
        {
            PositionNode(node);
        }

        _consoleHelper.WriteDone();

        MoveNodesToPositiveCoordinates();
    }

    /// <summary>
    /// Set the shapes of the positioned nodes. Apply pseudo-3D skewing effect to polygons, and make circles become ellipses
    /// (use random number to determine of the given node is skewed or not)
    /// </summary>
    public void SetNodeShapes()
    {
        double noSkewProbability = 0.2;
        float skewFactor;

        foreach (DirectedGraphNode node in _nodes.Values.Where(node => node.IsPositioned))
        {
            SetNodeShape(node);

            float rotationRadians = -0.785f + (float)_random.NextDouble() * 1.57f; // Range of -π/4 to π/4 radians

            if (_random.NextDouble() < noSkewProbability)
            {
                skewFactor = 0.0f;
            }
            else
            {
                skewFactor = (_random.NextDouble() > 0.5 ? 1 : -1) * (0.1f + (float)_random.NextDouble() * 0.8f);
            }

            if (node.Shape.ShapeType == Enums.ShapeType.Circle)
            {
                node.Shape.ShapeType = Enums.ShapeType.Ellipse;

                float horizontalOffset = node.Shape.Radius * (skewFactor * 0.6f);     //reduce the skew impact for ellipses
                float horizontalRadius = node.Shape.Radius + horizontalOffset;
                float verticalRadius = node.Shape.Radius;

                node.Shape.EllipseConfig = ((node.Position.X, node.Position.Y),
                                            horizontalRadius,
                                            verticalRadius);

                continue;
            }

            for (int i = 0; i < node.Shape.PolygonVertices.Count; i++)
            {
                node.Shape.PolygonVertices[i] = ApplyPerspectiveSkewToVertex(node.Shape.PolygonVertices[i],
                                                                             node.Position,
                                                                             skewFactor,
                                                                             rotationRadians);
            }
        }
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        if (node.IsPositioned)
        {
            return;
        }

        int allNodesAtDepth =
            _nodes.Values.Count(n => n.Depth == node.Depth);

        int positionedNodesAtDepth =
            _nodes.Values.Count(n => n.Depth == node.Depth && n.IsPositioned);

        float baseRadius = _settings.NodeRadius;

        if (node.Parent != null && node.Parent.Shape.Radius > 0)
        {
            baseRadius = node.Parent.Shape.Radius;
        }

        float maxZ = _nodes.Values.Max(node => node.Z);
        float depthFactor = node.Z / maxZ;
        float scale = 0.99f - depthFactor * 0.1f;
        float minScale = (float)0.2;
        float nodeRadius = baseRadius * Math.Max(scale - 0.02f, minScale);
        float xNodeSpacer = _settings.XNodeSpacer;
        float yNodeSpacer = _settings.YNodeSpacer;

        if (node.Depth < 10)
        {
            xNodeSpacer *= node.Depth;
            yNodeSpacer *= node.Depth;
        }

        float xOffset = node.Parent == null
                                ? 0
                                : node.Parent.Position.X;


        if (node.Parent!.Children.Count == 1)
        {
            xOffset = node.Parent.Position.X;
            nodeRadius = node.Parent.Shape.Radius;
        }
        else
        {
            int addedWidth;

            if (allNodesAtDepth % 2 == 0)
            {
                addedWidth = positionedNodesAtDepth == 0 ? 0 : positionedNodesAtDepth + 1;
            }
            else
            {
                addedWidth = positionedNodesAtDepth == 0 ? 0 : positionedNodesAtDepth;
            }

            if (node.IsFirstChild)
            {
                xOffset = xOffset - (allNodesAtDepth / 2 * xNodeSpacer) - (xNodeSpacer * addedWidth);
                node.Z -= 35;
                nodeRadius = node.Parent.Shape.Radius * Math.Max(scale, minScale);
            }
            else
            {
                xOffset = xOffset + (allNodesAtDepth / 2 * xNodeSpacer) + (xNodeSpacer * addedWidth);
                node.Z += 15;
                nodeRadius = node.Parent.Shape.Radius * Math.Max(scale - 0.02f, minScale);
            }
        }

        float yOffset = node.Parent!.Position.Y - (yNodeSpacer + yNodeSpacer / node.Depth + (positionedNodesAtDepth * (yNodeSpacer / 30)));

        node.Position = (xOffset, (float)yOffset);

        if (_settings.NodeRotationAngle != 0)
        {
            (float x, float y) = RotateNode(node.NumberValue, _settings.NodeRotationAngle, xOffset, yOffset);

            node.Position = (x, y);
        }

        if (node.Parent != null && node.Parent.Children.Count == 2)
        {
            node.Position = ApplyPerspectiveTransformToNodePosition(node, _settings.DistanceFromViewer);
        }

        node.Shape.Radius = nodeRadius;
        node.IsPositioned = true;
        _nodesPositioned += 1;

        _consoleHelper.Write($"\r{_nodesPositioned} nodes positioned... ");

        foreach (var childNode in node.Children)
        {
            PositionNode(childNode);
        }
    }

    /// <summary>
    /// For the pseudo-3D graph, apply depth to the given node based on the Z coordinate.
    /// The Z-coordinate is set to the reverse of the depth value of the node in the DirectedGraph.AddSeries() method
    /// </summary>
    /// <param name="node"></param>
    /// <param name="viewerDistance">The distance to the viewer</param>
    /// <returns></returns>
    private (float X, float Y) ApplyPerspectiveTransformToNodePosition(DirectedGraphNode node, float viewerDistance)
    {
        float xPrime = node.Position.X / (1 + node.Z / viewerDistance);
        float yPrime = node.Position.Y / (1 + node.Z / viewerDistance) - (_settings.YNodeSpacer * 4);

        return (xPrime, yPrime);
    }

    /// <summary>
    /// Apply a skew to a vertex of a polygon to give a pseudo-3D effect to the node shape
    /// </summary>
    /// <param name="vertex"></param>
    /// <param name="center"></param>
    /// <param name="skewFactor"></param>
    /// <param name="rotationRadians"></param>
    /// <returns></returns>
    private static (float X, float Y) ApplyPerspectiveSkewToVertex((float X, float Y) vertex,
                                                                   (float X, float Y) center,
                                                                   float skewFactor,
                                                                   float rotationRadians)
    {
        float dx = vertex.X - center.X;
        float dy = vertex.Y - center.Y;

        float rotatedX = dx * (float)Math.Cos(rotationRadians) - dy * (float)Math.Sin(rotationRadians);
        float rotatedY = dx * (float)Math.Sin(rotationRadians) + dy * (float)Math.Cos(rotationRadians);

        float skewedX = rotatedX + skewFactor * rotatedY;
        float skewedY = rotatedY;

        return (center.X + skewedX, center.Y + skewedY);
    }
}