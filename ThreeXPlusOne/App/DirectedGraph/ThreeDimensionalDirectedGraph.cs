using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.Shapes;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public class ThreeDimensionalDirectedGraph(IOptions<AppSettings> appSettings,
                                           IEnumerable<IDirectedGraphService> graphServices,
                                           ILightSourceService lightSourceService,
                                           IConsoleService consoleService,
                                           ShapeFactory shapeFactory)
                                                : DirectedGraph(appSettings, graphServices, lightSourceService, consoleService, shapeFactory),
                                                  IDirectedGraph
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
        (double X, double Y) base1 = (0, 0);                                                                   // Node '1' at the bottom
        (double X, double Y) base2 = (0, base1.Y - (_appSettings.NodeAestheticSettings.NodeSpacerY * 6));      // Node '2' just above '1'
        (double X, double Y) base4 = (0, base2.Y - (_appSettings.NodeAestheticSettings.NodeSpacerY * 5));      // Node '4' above '2'

        _nodes[1].Position = base1;
        _nodes[1].Position = ApplyNodePerspectiveTransformation(_nodes[1], _appSettings.DirectedGraphAestheticSettings.Pseudo3DViewerDistance);
        _nodes[1].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;
        _nodes[1].IsPositioned = true;

        _nodes[2].Position = base2;
        _nodes[2].Position = ApplyNodePerspectiveTransformation(_nodes[2], _appSettings.DirectedGraphAestheticSettings.Pseudo3DViewerDistance);
        _nodes[2].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;
        _nodes[2].IsPositioned = true;

        _nodes[4].Position = base4;
        _nodes[4].Position = ApplyNodePerspectiveTransformation(_nodes[4], _appSettings.DirectedGraphAestheticSettings.Pseudo3DViewerDistance);
        _nodes[4].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;
        _nodes[4].IsPositioned = true;

        _nodesPositioned = 3;

        //recursive method to position a nodes and its children
        PositionNode(_nodes[1]);

        _consoleService.WriteDone();

        NodePositions.MoveNodesToPositiveCoordinates(_nodes,
                                                     _appSettings.NodeAestheticSettings.NodeSpacerX,
                                                     _appSettings.NodeAestheticSettings.NodeSpacerY,
                                                     _appSettings.NodeAestheticSettings.NodeRadius);
    }

    /// <summary>
    /// Set the shapes and colours of the positioned nodes. Apply a pseudo-3D skewing effect to shapes.
    /// (use a random number to determine if the given node is skewed or not)
    /// </summary>
    public void SetNodeAesthetics()
    {
        double noSkewProbability = 0.2;
        int lcv = 1;

        foreach (DirectedGraphNode node in _nodes.Values.Where(node => node.IsPositioned))
        {
            _nodeAesthetics.SetNodeShape(node,
                                         _appSettings.NodeAestheticSettings.NodeRadius,
                                         _appSettings.NodeAestheticSettings.NodeShapes);

            if (Random.Shared.NextDouble() >= noSkewProbability)
            {
                node.Shape.SetShapeSkew(node.Position, node.Shape.Radius);
            }

            _nodeAesthetics.SetNodeColor(node,
                                         _appSettings.NodeAestheticSettings.NodeColors,
                                         _appSettings.NodeAestheticSettings.NodeColorsBias,
                                         _appSettings.NodeAestheticSettings.ColorCodeNumberSeries);

            _consoleService.Write($"\r{lcv} nodes styled... ");

            lcv++;
        }

        _consoleService.WriteDone();
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        if (!node.IsPositioned)
        {
            int allNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth);

            int positionedNodesAtDepth =
                _nodes.Values.Count(n => n.Depth == node.Depth && n.IsPositioned);

            double baseRadius = _appSettings.NodeAestheticSettings.NodeRadius;

            if (node.Parent != null && node.Parent.Shape.Radius > 0)
            {
                baseRadius = node.Parent.Shape.Radius;
            }

            double maxZ = _nodes.Values.Max(node => node.Z);
            double depthFactor = node.Z / maxZ;
            double scale = 0.99 - depthFactor * 0.1;
            double minScale = 0.2;
            double nodeRadius = baseRadius * Math.Max(scale - 0.02, minScale);
            double xNodeSpacer = _appSettings.NodeAestheticSettings.NodeSpacerX;
            double yNodeSpacer = _appSettings.NodeAestheticSettings.NodeSpacerY;

            //reduce bunching of nodes at lower depths by increasing the node spacers
            if (node.Depth < 10)
            {
                xNodeSpacer *= node.Depth;
                yNodeSpacer *= node.Depth;
            }

            double xOffset = node.Parent == null
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
                    nodeRadius = node.Parent.Shape.Radius * Math.Max(scale - 0.02, minScale);
                }
            }

            double yOffset = node.Parent!.Position.Y - (yNodeSpacer + yNodeSpacer / node.Depth + (positionedNodesAtDepth * (yNodeSpacer / 30)));

            node.Position = (xOffset, yOffset);

            if (_appSettings.NodeAestheticSettings.NodeRotationAngle != 0)
            {
                (double x, double y) = NodePositions.RotateNode(node.NumberValue,
                                                                _appSettings.NodeAestheticSettings.NodeRotationAngle,
                                                                xOffset,
                                                                yOffset);

                node.Position = (x, y);
            }

            if (node.Parent != null && node.Parent.Children.Count == 2)
            {
                node.Position = ApplyNodePerspectiveTransformation(node,
                                                                   _appSettings.DirectedGraphAestheticSettings.Pseudo3DViewerDistance);
            }

            node.Shape.Radius = nodeRadius;
            node.IsPositioned = true;
            _nodesPositioned += 1;

            _consoleService.Write($"\r{_nodesPositioned} nodes positioned... ");
        }

        foreach (DirectedGraphNode childNode in node.Children)
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
    private (double X, double Y) ApplyNodePerspectiveTransformation(DirectedGraphNode node,
                                                                    double viewerDistance)
    {
        double xPrime = node.Position.X / (1 + node.Z / viewerDistance);
        double yPrime = node.Position.Y / (1 + node.Z / viewerDistance) - (_appSettings.NodeAestheticSettings.NodeSpacerY * 4);

        return (xPrime, yPrime);
    }
}
