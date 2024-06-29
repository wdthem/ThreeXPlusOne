using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.Shapes;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public class ThreeDimensionalDirectedGraph(IOptions<AppSettings> appSettings,
                                           IEnumerable<IDirectedGraphDrawingService> directedGraphDrawingServices,
                                           ILightSourceService lightSourceService,
                                           IConsoleService consoleService,
                                           ShapeFactory shapeFactory)
                                                : DirectedGraph(appSettings, directedGraphDrawingServices, lightSourceService, consoleService, shapeFactory),
                                                  IDirectedGraph
{
    private readonly Dictionary<(int, int, int), List<(double X, double Y, double Z)>> _nodeGrid = [];

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
        //the Z coordinate is only applicable to the pseudo-3D graph, so set it up here as a first step
        SetNodeZValue(_nodes[1]);

        // Set up the base node's position
        (double X, double Y) baseNodePosition = (0, 0);

        _nodes[1].Position = baseNodePosition;
        _nodes[1].Shape.Radius = _appSettings.NodeAestheticSettings.NodeRadius;
        _nodes[1].IsPositioned = true;

        _nodesPositioned = 1;

        //recursive method to position a node and its children
        PositionNode(_nodes[1]);

        _consoleService.WriteDone();

        NodePositions.MoveNodesToPositiveCoordinates(_nodes,
                                                     _appSettings.NodeAestheticSettings.NodeSpacerX,
                                                     _appSettings.NodeAestheticSettings.NodeSpacerY,
                                                     _appSettings.NodeAestheticSettings.NodeRadius);
    }

    /// <summary>
    /// Set the shapes and colours of the positioned nodes. 
    /// Apply a pseudo-3D skewing effect to shapes.
    /// </summary>
    public void SetNodeAesthetics()
    {
        int lcv = 1;

        foreach (DirectedGraphNode node in _nodes.Values.Where(node => node.IsPositioned))
        {
            _nodeAesthetics.SetNodeShape(node,
                                         _appSettings.NodeAestheticSettings.NodeRadius,
                                         _appSettings.NodeAestheticSettings.NodeShapes);

            node.Shape.SetShapeSkew(node.Position, node.Shape.Radius);

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
    /// Traverse the node tree recursively and set the Z value for all nodes, based on whether the node has 1 or 2 children
    /// </summary>
    /// <remarks>
    /// Used for the pseudo-3D graph
    /// </remarks>
    /// <param name="node"></param>
    private static void SetNodeZValue(DirectedGraphNode node)
    {
        if (node.Parent == null)
        {
            node.Z = 0;
        }
        else if (node.Parent.Children.Count == 2)
        {
            if (node.IsFirstChild)
            {
                node.Z = node.Parent.Z - 1;
            }
            else
            {
                node.Z = node.Parent.Z + 1;
            }
        }
        else
        {
            node.Z = node.Parent.Z;
        }

        foreach (DirectedGraphNode childNode in node.Children)
        {
            SetNodeZValue(childNode);
        }
    }

    /// <summary>
    /// Recursive method to position node and all its children down the tree
    /// </summary>
    /// <param name="node"></param>
    private void PositionNode(DirectedGraphNode node)
    {
        if (!node.IsPositioned)
        {
            double baseRadius = node.Parent != null && node.Parent.Shape.Radius > 0
                                        ? node.Parent.Shape.Radius
                                        : _appSettings.NodeAestheticSettings.NodeRadius;

            double maxZ = _nodes.Values.Max(node => node.Z);
            double minZ = _nodes.Values.Min(node => node.Z);

            double normalizedZ = NormalizeZ(node.Z, minZ, maxZ);

            double nodeRadius = baseRadius;

            if (node.Z != node.Parent?.Z)
            {
                double scale = Math.Exp(normalizedZ / _appSettings.DirectedGraphAestheticSettings.Pseudo3DViewerDistance) * 1.1;
                nodeRadius = baseRadius / scale;
            }

            double xNodeSpacer = _appSettings.NodeAestheticSettings.NodeSpacerX;
            double yNodeSpacer = _appSettings.NodeAestheticSettings.NodeSpacerY;

            double xOffset = node.Parent == null
                                    ? 0
                                    : node.Parent.Position.X;

            double yOffset = node.Parent == null
                                    ? 0
                                    : node.Parent.Position.Y - yNodeSpacer;

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
                                                                   _appSettings.DirectedGraphAestheticSettings.Pseudo3DViewerDistance,
                                                                   xNodeSpacer,
                                                                   yNodeSpacer,
                                                                   minZ,
                                                                   maxZ);
            }

            double minDistance = _appSettings.NodeAestheticSettings.NodeRadius;

            while (NodeOverlapsNeighbours(node, minDistance))
            {
                // first move the node to the right
                node.Position = (node.Position.X + _appSettings.NodeAestheticSettings.NodeSpacerX, node.Position.Y);

                // now check if moving to the right caused node overlap
                if (NodeOverlapsNeighbours(node, minDistance))
                {
                    // move to the left instead
                    node.Position = (node.Position.X - 2 * _appSettings.NodeAestheticSettings.NodeSpacerX, node.Position.Y);
                }
            }

            AddNodeToGrid(node, minDistance);

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
    /// </summary>
    /// <param name="node"></param>
    /// <param name="viewerDistance">The distance to the viewer</param>
    /// <returns></returns>
    private static (double X, double Y) ApplyNodePerspectiveTransformation(DirectedGraphNode node,
                                                                       double viewerDistance,
                                                                       double horizontalSpacing,
                                                                       double verticalSpacing,
                                                                       double minZ,
                                                                       double maxZ)
    {
        double normalizedZ = NormalizeZ(node.Z, minZ, maxZ);

        double scale = 1 / (1 + normalizedZ / viewerDistance);
        double xPrime = node.Position.X * scale;
        double yPrime = node.Position.Y * scale;

        if (node.IsFirstChild)
        {
            xPrime -= horizontalSpacing * scale;
            yPrime += verticalSpacing * scale; // Slightly below
        }
        else
        {
            xPrime += horizontalSpacing * scale;
            yPrime -= verticalSpacing * scale; // Slightly above
        }

        return (xPrime, yPrime);
    }

    /// <summary>
    /// Determine if the node that was just positioned is too close to neighbouring nodes (and thus overlapping)
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    private bool NodeOverlapsNeighbours(DirectedGraphNode newNode, double minDistance)
    {
        int maxNodeDepth = _nodes.Values.Max(node => node.Depth);
        int minNodeDepth = _nodes.Values.Min(node => node.Depth);

        (int, int, int) cell = GetGridCellForNode(newNode, minDistance);

        double allowOverlapProbability = 0.1;

        // Check this cell and adjacent cells
        foreach ((int, int, int) offset in new[]
            {
                (0, 0, 0), (1, 0, 0), (0, 1, 0), (0, 0, 1),
                (-1, 0, 0), (0, -1, 0), (0, 0, -1),
                (1, 1, 0), (1, 0, 1), (0, 1, 1),
                (-1, -1, 0), (-1, 0, -1), (0, -1, -1),
                (1, -1, 0), (-1, 1, 0), (1, 0, -1), (-1, 0, 1),
                (0, 1, -1), (0, -1, 1)
            })
        {
            (int, int, int) checkCell = (cell.Item1 + offset.Item1,
                                         cell.Item2 + offset.Item2,
                                         cell.Item3 + offset.Item3);

            if (_nodeGrid.TryGetValue(checkCell, out var nodesInCell))
            {
                foreach ((double X, double Y, double Z) in nodesInCell)
                {
                    double normalizedZ1 = NormalizeZ(newNode.Z, minNodeDepth, maxNodeDepth);
                    double normalizedZ2 = NormalizeZ(Z, minNodeDepth, maxNodeDepth);

                    if (Distance((newNode.Position.X, newNode.Position.Y, normalizedZ1), (X, Y, normalizedZ2)) < minDistance)
                    {
                        if (Random.Shared.NextDouble() < allowOverlapProbability)
                        {
                            return false; // Allow overlap
                        }

                        return true;
                    }
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Retrieve the cell in the grid object in which the node is positioned
    /// </summary>
    /// <param name="node"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    private static (int, int, int) GetGridCellForNode(DirectedGraphNode node, double cellSize)
    {
        int xCell = (int)(node.Position.X / cellSize);
        int yCell = (int)(node.Position.Y / cellSize);
        int zCell = (int)(node.Z / cellSize);

        return (xCell, yCell, zCell);
    }

    /// <summary>
    /// Add the node to the grid dictionary to keep track of node positions via a grid system
    /// </summary>
    /// <param name="node"></param>
    /// <param name="minDistance"></param>
    private void AddNodeToGrid(DirectedGraphNode node, double minDistance)
    {
        (int, int, int) cell = GetGridCellForNode(node, minDistance);

        if (!_nodeGrid.TryGetValue(cell, out List<(double X, double Y, double Z)>? value))
        {
            value = [];
            _nodeGrid[cell] = value;
        }

        value.Add((node.Position.X, node.Position.Y, node.Z));
    }

    /// <summary>
    /// Calculate the Euclidean distance between two node positions, including the third (Z) dimension
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns></returns>
    private static double Distance((double X, double Y, double Z) pos1, (double X, double Y, double Z) pos2)
    {
        double dx = pos1.X - pos2.X;
        double dy = pos1.Y - pos2.Y;
        double dz = pos1.Z - pos2.Z;

        return Math.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    /// <summary>
    /// Normalize the Z dimension to avoid drastic differences throughout the graph
    /// </summary>
    /// <param name="z"></param>
    /// <param name="minZ"></param>
    /// <param name="maxZ"></param>
    /// <returns></returns>
    private static double NormalizeZ(double z, double minZ, double maxZ)
    {
        if (maxZ == minZ)
        {
            return 0.5; // If all Z values are the same, return a neutral value
        }
        return (z - minZ) / (maxZ - minZ);
    }
}