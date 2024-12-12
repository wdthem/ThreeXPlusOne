using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;
using ThreeXPlusOne.App.Presenters.Interfaces;

namespace ThreeXPlusOne.App.Services;

public class DirectedGraphService(ILogger<DirectedGraphService> logger,
                                  IOptions<AppSettings> appSettings,
                                  IEnumerable<IDirectedGraph> directedGraphs,
                                  IAlgorithmService algorithmService,
                                  IDirectedGraphPresenter directedGraphPresenter) : IDirectedGraphService
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Generate the directed graph based on app settings.
    /// </summary>
    public async Task GenerateDirectedGraph()
    {
        if (!Enum.TryParse(_appSettings.DirectedGraphAestheticSettings.GraphType, out GraphType graphType))
        {
            throw new ApplicationException("Invalid graph type specified.");
        }

        List<CollatzResult> collatzResults = await algorithmService.Run();

        IDirectedGraph graph = directedGraphs.ToList()
                                             .Where(graph => graph.GraphType == graphType)
                                             .First();

        directedGraphPresenter.DisplayHeading($"Directed graph");

        graph.AddSeries(graphType, collatzResults);
        graph.PositionNodes();
        graph.SetNodeAesthetics();
        graph.SetCanvasDimensions();

        if (!GraphGenerationConfirmed(graphType))
        {
            return;
        }

        await graph.Draw();

        logger.LogInformation("Directed graph ({GraphType}) generated successfully", graphType);
    }

    /// <summary>
    /// Confirm the user wants to generate the graph.
    /// </summary>
    /// <param name="graphType">The type of graph to generate.</param>
    private bool GraphGenerationConfirmed(GraphType graphType)
    {
        //allow the user to bail on generating the graph (for example, if canvas dimensions are too large)
        bool confirmedGenerateGraph = directedGraphPresenter.GetConfirmation($"\nGenerate {graphType} visualisation?");

        if (!confirmedGenerateGraph)
        {
            directedGraphPresenter.DisplayGraphGenerationCancelledMessage();

            return false;
        }

        return true;
    }
}