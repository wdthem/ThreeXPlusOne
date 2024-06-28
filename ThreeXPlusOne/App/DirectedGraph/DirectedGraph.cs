﻿using Microsoft.Extensions.Options;
using System.Drawing;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.Shapes;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.DirectedGraph;

public abstract partial class DirectedGraph(IOptions<AppSettings> appSettings,
                                            IEnumerable<IDirectedGraphDrawingService> graphServices,
                                            ILightSourceService lightSourceService,
                                            IConsoleService consoleService,
                                            ShapeFactory shapeFactory)
{
    private int _canvasWidth = 0;
    private int _canvasHeight = 0;

    protected readonly AppSettings _appSettings = appSettings.Value;
    protected readonly IConsoleService _consoleService = consoleService;
    protected readonly NodePositions _nodePositions = new();
    protected readonly NodeAesthetics _nodeAesthetics = new(shapeFactory);
    protected readonly Dictionary<int, DirectedGraphNode> _nodes = [];

    /// <summary>
    /// Add multiple series of numbers to the graph generated by the algorithm
    /// </summary>
    /// <param name="seriesLists"></param>
    public void AddSeries(List<List<int>> seriesLists)
    {
        _consoleService.Write($"Adding {seriesLists.Count} series to the graph... ");

        int lcv = 1;

        foreach (List<int> series in seriesLists)
        {
            DirectedGraphNode? previousNode = null;
            int currentDepth = series.Count;

            foreach (int number in series)
            {
                //only add a new node to the dictionary if the number value is not already there,
                // as each number can only have one node on the graph, even if it is repeated across series
                if (!_nodes.TryGetValue(number, out DirectedGraphNode? currentNode))
                {
                    currentNode = new DirectedGraphNode(number)
                    {
                        Depth = currentDepth,
                        SeriesNumber = lcv  //only set the series number if this is a new node
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

                previousNode = currentNode;

                currentDepth--;
            }

            lcv++;
        }

        if (_appSettings.DirectedGraphAestheticSettings.SanitizedGraphDimensions == 3)
        {
            SetNodeZValue(_nodes[1]);
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
    /// Draw the directed graph
    /// </summary>
    /// <exception cref="Exception"></exception>
    protected void DrawDirectedGraph()
    {
        IDirectedGraphDrawingService graphService = graphServices.ToList()
                                                          .Where(graphService => graphService.GraphProvider == _appSettings.GraphProvider)
                                                          .First();

        if (!graphService.SupportedDimensions.Contains(_appSettings.DirectedGraphAestheticSettings.SanitizedGraphDimensions))
        {
            throw new Exception($"Graph provider {_appSettings.GraphProvider} does not support graphs in {_appSettings.DirectedGraphAestheticSettings.SanitizedGraphDimensions} dimensions.");
        }

        ConfigureGraphServiceActions(graphService);

        Task.Run(() => graphService.Initialize([.. _nodes.Values],
                                               _canvasWidth,
                                               _canvasHeight,
                                               GetCanvasColor())).Wait();


        if (_appSettings.DirectedGraphAestheticSettings.GenerateBackgroundStars)
        {
            Task.Run(() => graphService.GenerateBackgroundStars(100)).Wait();
        }

        lightSourceService.Initialize(_canvasWidth,
                                      _canvasHeight,
                                      _appSettings.DirectedGraphAestheticSettings.LightSourcePosition,
                                      _appSettings.DirectedGraphAestheticSettings.LightSourceColor);

        if (lightSourceService.LightSourcePosition != LightSourcePosition.None)
        {
            Task.Run(() => graphService.GenerateLightSource(lightSourceService.GetLightSourceCoordinates(lightSourceService.LightSourcePosition),
                                                            lightSourceService.Radius,
                                                            lightSourceService.LightSourceColor)).Wait();

            NodeAesthetics.ApplyLightSourceToNodes(_nodes,
                                                   lightSourceService.GetLightSourceCoordinates(lightSourceService.LightSourcePosition),
                                                   lightSourceService.GetLightSourceMaxDistanceOfEffect(),
                                                   lightSourceService.LightSourceColor);
        }

        Task.Run(() => graphService.Draw(drawNumbersOnNodes: _appSettings.NodeAestheticSettings.DrawNumbersOnNodes,
                                         drawNodeConnections: _appSettings.NodeAestheticSettings.DrawNodeConnections)).Wait();

        Task.Run(graphService.Render).Wait();

        //saving the image is processing-intensive and can cause threading issues, so start it via Task.Factory
        //in order to specify that it is expected to be long-running
        Task.Factory.StartNew(graphService.SaveImage,
                              CancellationToken.None,
                              TaskCreationOptions.LongRunning,
                              TaskScheduler.Default).Wait();

        Task.Run(graphService.Dispose).Wait();
    }

    /// <summary>
    /// Set the canvas dimensions to a bit more than the bounding box of all the nodes
    /// </summary>
    protected void SetCanvasSize()
    {
        double maxX = _nodes.Values.Max(node => node.Position.X);
        double maxY = _nodes.Values.Max(node => node.Position.Y);

        double maxNodeRadius = _nodes.Values.Max(node => node.Shape.Radius);

        _canvasWidth = (int)(maxX + _appSettings.NodeAestheticSettings.NodeSpacerX + maxNodeRadius);
        _canvasHeight = (int)(maxY + _appSettings.NodeAestheticSettings.NodeSpacerY + maxNodeRadius);

        _consoleService.Write($"Setting canvas dimensions to {_canvasWidth}w x {_canvasHeight}h... ");
        _consoleService.WriteDone();
    }

    /// <summary>
    /// Get the a colour object to use for the canvas background colour
    /// </summary>
    /// <returns></returns>
    private Color GetCanvasColor()
    {
        if (string.IsNullOrWhiteSpace(_appSettings.DirectedGraphAestheticSettings.CanvasColor))
        {
            return Color.Black;
        }

        Color colorFromHexCode = ColorTranslator.FromHtml(_appSettings.DirectedGraphAestheticSettings.CanvasColor);

        if (colorFromHexCode == Color.Empty)
        {
            return Color.Black;
        }

        return Color.FromArgb(255,                  //fully opaque background
                              colorFromHexCode.R,
                              colorFromHexCode.G,
                              colorFromHexCode.B);
    }

    /// <summary>
    /// Configure actions of the directed graph service
    /// </summary>
    /// <param name="graphService"></param>
    private void ConfigureGraphServiceActions(IDirectedGraphDrawingService graphService)
    {
        graphService.OnStart = (message) =>
        {
            _consoleService.Write(message);
            _consoleService.ShowSpinningBar();
        };

        graphService.OnComplete = () =>
        {
            _consoleService.StopSpinningBar();
            _consoleService.WriteDone();
        };
    }

    /// <summary>
    /// Calculate the Euclidean distance between two node positions
    /// </summary>
    /// <param name="position1"></param>
    /// <param name="position2"></param>
    /// <returns></returns>
    protected static double Distance((double X, double Y) position1,
                                   (double X, double Y) position2)
    {
        return Math.Sqrt(Math.Pow(position2.X - position1.X, 2) + Math.Pow(position2.Y - position1.Y, 2));
    }
}