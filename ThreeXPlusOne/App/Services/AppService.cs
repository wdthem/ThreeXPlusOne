using System.Diagnostics;
using Microsoft.Extensions.Logging;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Presenters.Interfaces;

namespace ThreeXPlusOne.App.Services;

public class AppService(ILogger<AppService> logger,
                        IDirectedGraphService directedGraphService,
                        IAppSettingsService appSettingsService,
                        IAppPresenter appPresenter) : IScopedService
{
    /// <summary>
    /// Generated the directed graph based on the user-provided app settings.
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    public async Task Run(List<string> commandParsingMessages)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        appPresenter.DisplayAppHeader();
        appPresenter.DisplayCommandParsingMessages(commandParsingMessages);

        await directedGraphService.GenerateDirectedGraph();
        await appSettingsService.UpdateAppSettingsFile();

        stopwatch.Stop();

        appPresenter.DisplayProcessEnd(stopwatch.Elapsed);

        logger.LogInformation("Process completed in {ElapsedTime}", stopwatch.Elapsed);
    }
}