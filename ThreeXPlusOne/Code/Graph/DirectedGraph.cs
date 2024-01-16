﻿using Microsoft.Extensions.Options;
using System.Drawing;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;
using ThreeXPlusOne.Enums;
using ThreeXPlusOne.Models;

namespace ThreeXPlusOne.Code.Graph;

public abstract class DirectedGraph(IOptions<Settings> settings,
                                    IEnumerable<IDirectedGraphService> graphServices,
                                    IConsoleHelper consoleHelper)
{
    private int _canvasWidth = 0;
    private int _canvasHeight = 0;
    private readonly Dictionary<(int, int), List<(float X, float Y)>> _nodeGrid = [];

    protected readonly Random _random = new();
    protected readonly Settings _settings = settings.Value;
    protected readonly IConsoleHelper _consoleHelper = consoleHelper;
    protected readonly Dictionary<int, DirectedGraphNode> _nodes = [];

    /// <summary>
    /// Add multiple series of numbers to the graph generated by the algorithm
    /// </summary>
    /// <param name="seriesLists"></param>
    public void AddSeries(List<List<int>> seriesLists)
    {
        _consoleHelper.Write($"Adding {seriesLists.Count} series to the graph... ");

        foreach (List<int> series in seriesLists)
        {
            DirectedGraphNode? previousNode = null;
            int currentDepth = series.Count;

            foreach (var number in series)
            {
                if (!_nodes.TryGetValue(number, out DirectedGraphNode? currentNode))
                {
                    currentNode = new DirectedGraphNode(number)
                    {
                        Depth = currentDepth
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
                        previousNode.IsFirstChild = true;

                        //if this node already has a child, turn off the flag
                        if (currentNode.Children.Count == 1)
                        {
                            previousNode.IsFirstChild = false;
                        }

                        currentNode.Children.Add(previousNode);
                    }
                }

                currentNode.Shape.Color = GenerateNodeColor();

                previousNode = currentNode;

                currentDepth--;
            }

            int maxNodeDepth = _nodes.Max(node => node.Value.Depth);

            foreach (var node in _nodes)
            {
                node.Value.Z = maxNodeDepth - node.Value.Depth;
            }
        }

        _consoleHelper.WriteDone();
    }

    /// <summary>
    /// Draw the directed graph
    /// </summary>
    protected void DrawDirectedGraph()
    {
        IDirectedGraphService graphService = graphServices.ToList()
                                                          .Where(graphService => graphService.GraphProvider == _settings.GraphProvider)
                                                          .First();

        if (!graphService.SupportedDimensions.Contains(_settings.SanitizedGraphDimensions))
        {
            throw new Exception($"Graph provider {_settings.GraphProvider} does not support graphs in {_settings.SanitizedGraphDimensions} dimensions.");
        }

        graphService.Initialize([.. _nodes.Values],
                                _canvasWidth,
                                _canvasHeight);

        if (_settings.GenerateBackgroundStars)
        {
            graphService.GenerateBackgroundStars(100);
        }

        if (_settings.GenerateLightSource)
        {
            graphService.GenerateLightSource();
        }

        graphService.Draw(drawNumbersOnNodes: _settings.DrawNumbersOnNodes,
                          drawNodeConnections: _settings.DrawConnections);

        graphService.Render();
        graphService.SaveImage();
        graphService.Dispose();
    }

    /// <summary>
    /// The graph starts out at 0,0 with 0 width and 0 height. This means that nodes go into negative space as they are initially positioned, 
    /// so all coordinates need to be shifted to make sure all are in positive space
    /// </summary>
    protected void MoveNodesToPositiveCoordinates()
    {
        _consoleHelper.Write("Adjusting node positions to fit on canvas... ");

        float minX = _nodes.Min(node => node.Value.Position.X);
        float minY = _nodes.Min(node => node.Value.Position.Y);

        float translationX = minX < 0 ? -minX + _settings.XNodeSpacer + (int)_settings.NodeRadius : 0;
        float translationY = minY < 0 ? -minY + _settings.YNodeSpacer + (int)_settings.NodeRadius : 0;

        foreach (var node in _nodes)
        {
            node.Value.Position = (node.Value.Position.X + translationX,
                                   node.Value.Position.Y + translationY);
        }

        _consoleHelper.WriteDone();
    }

    /// <summary>
    /// Set the canvas dimensions to a bit more than the bounding box of all the nodes
    /// </summary>
    protected void SetCanvasSize()
    {
        float maxX = _nodes.Max(node => node.Value.Position.X);
        float maxY = _nodes.Max(node => node.Value.Position.Y);

        _canvasWidth = (int)maxX + _settings.XNodeSpacer + (int)_settings.NodeRadius;
        _canvasHeight = (int)maxY + _settings.YNodeSpacer + (int)_settings.NodeRadius;

        _consoleHelper.WriteLine($"Canvas dimensions set to {_canvasWidth}w x {_canvasHeight}h (in pixels)\n");
    }

    /// <summary>
    /// Determine if the node that was just positioned is too close to neighbouring nodes (and thus overlapping)
    /// </summary>
    /// <param name="newNode"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    protected bool NodeIsTooCloseToNeighbours(DirectedGraphNode newNode,
                                              float minDistance)
    {
        (int, int) cell = GetGridCellForNode(newNode, minDistance);

        // Check this cell and adjacent cells
        foreach ((int, int) offset in new[] { (0, 0), (1, 0), (0, 1), (-1, 0), (0, -1) })
        {
            (int, int) checkCell = (cell.Item1 + offset.Item1,
                                    cell.Item2 + offset.Item2);

            if (_nodeGrid.TryGetValue(checkCell, out var nodesInCell))
            {
                foreach (var node in nodesInCell)
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
    protected void AddNodeToGrid(DirectedGraphNode node,
                                 float minDistance)
    {
        (int, int) cell = GetGridCellForNode(node, minDistance);

        if (!_nodeGrid.TryGetValue(cell, out List<(float X, float Y)>? value))
        {
            value = ([]);
            _nodeGrid[cell] = value;
        }

        value.Add(node.Position);
    }

    /// <summary>
    /// Calculate the Euclidean distance between two node positions
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns></returns>
    protected static float Distance((float X, float Y) position1,
                                    (float X, float Y) position2)
    {
        return (float)Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
    }

    /// <summary>
    /// Calculate the signed X-axis distance from a parent to a child node
    /// </summary>
    /// <remarks>
    /// Negative if child is the to left of the parent, positive if to the right
    /// </remarks>
    /// <param name="childPosition"></param>
    /// <param name="parentPosition"></param>
    /// <returns></returns>
    protected static float XAxisSignedDistanceFromParent((float X, float Y) childPosition,
                                                         (float X, float Y) parentPosition)
    {
        return childPosition.X - parentPosition.X;
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
    protected static (float X, float Y) RotateNode(int nodeValue,
                                                   float rotationAngle,
                                                   float x,
                                                   float y)
    {
        (float x, float y) rotatedPosition;

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
    /// Assign a ShapeType to the node and vertices if applicable
    /// </summary>
    /// <param name="node"></param>
    protected void SetNodeShape(DirectedGraphNode node)
    {
        if (node.Shape.Radius == 0)
        {
            node.Shape.Radius = _settings.NodeRadius;
        }

        int numberOfSides = _random.Next(0, 11);

        if (!_settings.IncludePolygonsAsNodes || numberOfSides == 0)
        {
            node.Shape.ShapeType = ShapeType.Circle;

            return;
        }

        if (numberOfSides == 1 || numberOfSides == 2)
        {
            numberOfSides = _random.Next(3, 11); //cannot have 1 or 2 sides, so re-select
        }

        node.Shape.ShapeType = ShapeType.Polygon;

        float rotationAngle = (float)(_random.NextDouble() * 2 * Math.PI);

        for (int i = 0; i < numberOfSides; i++)
        {
            float angle = (float)(2 * Math.PI / numberOfSides * i) + rotationAngle;

            node.Shape.Vertices.Add((node.Position.X + node.Shape.Radius * (float)Math.Cos(angle),
                                     node.Position.Y + node.Shape.Radius * (float)Math.Sin(angle)));
        }
    }

    /// <summary>
    /// Rotate the node's position clockwise based on the angle provided by the user. This gives a more artistic feel to the generated graph.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    private static (float x, float y) RotatePointClockwise(float x,
                                                           float y,
                                                           float angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x + sinTheta * y;
        double yNew = -sinTheta * x + cosTheta * y;

        return ((float)xNew, (float)yNew);
    }

    /// <summary>
    /// Rotate the node's position anti-clockwise based on the angle provided by the user. This gives a more artistic feel to the generated graph.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="angleDegrees"></param>
    /// <returns></returns>
    private static (float x, float y) RotatePointAntiClockwise(float x,
                                                               float y,
                                                               float angleDegrees)
    {
        double angleRadians = angleDegrees * Math.PI / 180.0; // Convert angle to radians

        double cosTheta = Math.Cos(angleRadians);
        double sinTheta = Math.Sin(angleRadians);

        double xNew = cosTheta * x - sinTheta * y;
        double yNew = sinTheta * x + cosTheta * y;

        return ((float)xNew, (float)yNew);
    }

    /// <summary>
    /// Retrieve the cell in the grid object in which the node is positioned
    /// </summary>
    /// <param name="node"></param>
    /// <param name="cellSize"></param>
    /// <returns></returns>
    private static (int, int) GetGridCellForNode(DirectedGraphNode node,
                                                 float cellSize)
    {
        return ((int)(node.Position.X / cellSize), (int)(node.Position.Y / cellSize));
    }

    /// <summary>
    /// Generate a random colour for the node
    /// </summary>
    /// <returns></returns>
    private Color GenerateNodeColor()
    {
        byte alpha = (byte)_random.Next(30, 211);
        byte red, green, blue;

        do
        {
            red = (byte)_random.Next(256);
            green = (byte)_random.Next(256);
            blue = (byte)_random.Next(256);
        }
        while (red <= 10 || green <= 10 || blue <= 10); //avoid very dark colours

        return Color.FromArgb(alpha, red, green, blue);
    }
}