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
                                  IConsoleService consoleService) : IDirectedGraphService
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

        consoleService.WriteHeading($"Directed graph ({graphType})");

        graph.AddSeries(collatzResults);
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
}