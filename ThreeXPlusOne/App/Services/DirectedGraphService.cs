using System.Diagnostics;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Enums;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services;

public class DirectedGraphService(IOptions<AppSettings> appSettings,
                                  IEnumerable<IDirectedGraph> directedGraphs,
                                  IAlgorithmService algorithmService,
                                  IMetadataService metadataService,
                                  IConsoleService consoleService) : IDirectedGraphService
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Generate the directed graph based on app settings.
    /// </summary>
    /// <param name="stopwatch"></param>
    public async Task GenerateDirectedGraph(Stopwatch stopwatch)
    {
        if (!Enum.TryParse(_appSettings.DirectedGraphAestheticSettings.GraphType, out GraphType graphType))
        {
            throw new ApplicationException("Invalid graph type specified.");
        }

        List<List<int>> seriesData = await GetSeriesData(stopwatch);

        IDirectedGraph graph = directedGraphs.ToList()
                                             .Where(graph => graph.GraphType == graphType)
                                             .First();

        consoleService.WriteHeading($"Directed graph ({graphType})");

        graph.AddSeries(seriesData);
        graph.PositionNodes();
        graph.SetCanvasDimensions();
        graph.SetNodeAesthetics();

        //allow the user to bail on generating the graph (for example, if canvas dimensions are too large)
        bool confirmedGenerateGraph = consoleService.ReadYKeyToProceed($"Generate {graphType} visualization?");

        if (!confirmedGenerateGraph)
        {
            consoleService.WriteLine("\nGraph generation cancelled\n");

            return;
        }

        consoleService.WriteLine("");

        await graph.Draw();
    }

    /// <summary>
    /// Get the series data from the algorithm service.
    /// </summary>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    private async Task<List<List<int>>> GetSeriesData(Stopwatch stopwatch)
    {
        List<CollatzResult> collatzResults = algorithmService.Run(stopwatch);
        List<List<int>> seriesData = collatzResults.Select(cr => cr.Values).ToList();

        await metadataService.GenerateMetadata(seriesData);

        return seriesData;
    }
}