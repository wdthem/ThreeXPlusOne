﻿using Microsoft.Extensions.Options;
using System.Drawing;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.DirectedGraph.NodeShapes;
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
    /// Add multiple series of numbers to the graph generated by the algorithm.
    /// </summary>
    /// <param name="collatzResults"></param>
    public void AddSeries(List<CollatzResult> collatzResults)
    {
        _consoleService.Write($"Adding {collatzResults.Count} series to the graph... ");

        for (int seriesNumber = 1; seriesNumber <= collatzResults.Count; seriesNumber++)
        {
            var series = collatzResults[seriesNumber - 1].Values;
            DirectedGraphNode? previousNode = null;

            for (int i = 0; i < series.Count; i++)
            {
                int number = series[i];
                int currentDepth = series.Count - i;

                //only add a new node to the dictionary if the number value is not already there,
                // as each number can only have one node on the graph, even if it is repeated across series
                if (!_nodes.TryGetValue(number, out DirectedGraphNode? currentNode))
                {
                    currentNode = new DirectedGraphNode(number)
                    {
                        Depth = currentDepth,
                        SeriesNumber = seriesNumber //only set the series number if this is a new node
                    };

                    _nodes.Add(number, currentNode);
                }

                // Check if this is a deeper path to the current node
                else if (currentDepth < currentNode.Depth)
                {
                    currentNode.Depth = currentDepth;
                }

                if (previousNode != null)
                {
                    previousNode.Parent = currentNode;

                    // Check if previousNode is already a child to prevent duplicate additions
                    if (!currentNode.Children.Contains(previousNode))
                    {
                        //if this node already has a child, turn off the flag
                        previousNode.IsFirstChild = currentNode.Children.Count == 0;
                        currentNode.Children.Add(previousNode);
                    }
                }

                previousNode = currentNode;
            }
        }

        _consoleService.WriteDone();
    }

    /// <summary>
    /// Draw the directed graph.
    /// </summary>
    /// <exception cref="Exception"></exception>
    protected async Task DrawDirectedGraph()
    {
        IDirectedGraphDrawingService graphService = graphServices.ToList()
                                                                 .Where(graphService => graphService.GraphProvider == _appSettings.GraphProvider)
                                                                 .First();

        ConfigureGraphServiceActions(graphService);

        await Task.Run(() => graphService.Initialize([.. _nodes.Values],
                                                     _canvasWidth,
                                                     _canvasHeight,
                                                     GetCanvasColor()));


        if (_appSettings.DirectedGraphAestheticSettings.GenerateBackgroundStars)
        {
            await Task.Run(() => graphService.GenerateBackgroundStars(100));
        }

        lightSourceService.Initialize(_canvasWidth,
                                      _canvasHeight,
                                      _appSettings.DirectedGraphAestheticSettings.LightSourcePosition,
                                      _appSettings.DirectedGraphAestheticSettings.LightSourceColor);

        if (lightSourceService.LightSourcePosition != LightSourcePosition.None)
        {
            await Task.Run(() => graphService.GenerateLightSource(lightSourceService.GetLightSourceCoordinates(lightSourceService.LightSourcePosition),
                                                                  lightSourceService.Radius,
                                                                  lightSourceService.LightSourceColor));

            NodeAesthetics.ApplyLightSourceToNodes(_nodes,
                                                   lightSourceService.GetLightSourceCoordinates(lightSourceService.LightSourcePosition),
                                                   lightSourceService.LightSourceColor,
                                                   lightSourceService.LightSourceIntensity);
        }

        await Task.Run(() => graphService.Draw(drawNumbersOnNodes: _appSettings.NodeAestheticSettings.DrawNumbersOnNodes,
                                               drawNodeConnections: _appSettings.NodeAestheticSettings.DrawNodeConnections));

        //saving the image is processing-intensive and can cause threading issues, so start it via Task.Factory
        //in order to specify that it is expected to be long-running
        await Task.Factory.StartNew(() => graphService.SaveImage(_appSettings.OutputFileType,
                                                                 _appSettings.OutputFileQuality),
                                    CancellationToken.None,
                                    TaskCreationOptions.LongRunning,
                                    TaskScheduler.Default);

        await Task.Run(graphService.Dispose);
    }

    /// <summary>
    /// Set the canvas dimensions to a bit more than the bounding box of all the nodes.
    /// </summary>
    protected void SetCanvasSize()
    {
        double maxX = _nodes.Values.Max(node => node.Position.X);
        double maxY = _nodes.Values.Max(node => node.Position.Y);

        double maxNodeRadius = _nodes.Values.Max(node => node.Shape.Radius);

        _canvasWidth = (int)(maxX + _appSettings.NodeAestheticSettings.NodeSpacerX + maxNodeRadius);
        _canvasHeight = (int)(maxY + _appSettings.NodeAestheticSettings.NodeSpacerY + maxNodeRadius);

        _consoleService.Write($"Setting canvas dimensions to {_canvasWidth:N0} x {_canvasHeight:N0}... ");
        _consoleService.WriteDone();
    }

    /// <summary>
    /// Get the a colour object to use for the canvas background colour.
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
    /// Configure actions of the directed graph service.
    /// </summary>
    /// <param name="graphService"></param>
    private void ConfigureGraphServiceActions(IDirectedGraphDrawingService graphService)
    {
        graphService.OnStart = (message) =>
        {
            _consoleService.SetForegroundColor(ConsoleColor.White);
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
    /// Calculate the Euclidean distance between two node positions.
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