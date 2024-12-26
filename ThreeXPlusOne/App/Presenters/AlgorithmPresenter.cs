using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Presenters.Interfaces;
using ThreeXPlusOne.App.Presenters.Interfaces.Components;
using ThreeXPlusOne.App.Services.Interfaces;

namespace ThreeXPlusOne.App.Presenters;

public class AlgorithmPresenter(IOptions<AppSettings> appSettings,
                                IConsoleService consoleService,
                                IUiComponent uiComponent) : IAlgorithmPresenter
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Display a heading with the given text.
    /// </summary>
    /// <param name="heading"></param>
    public void DisplayHeading(string heading)
    {
        uiComponent.WriteHeading(heading);
    }

    /// <summary>
    /// Display a message indicating that the algorithm is running.
    /// </summary>
    /// <param name="numberCount"></param>
    public void DisplayRunningAlgorithmMessage(int numberCount)
    {
        consoleService.WriteWithMarkup($"  Running 3x + 1 algorithm on {numberCount} numbers... ");
    }

    /// <summary>
    /// Display a message indicating that the series numbers are being used.
    /// </summary>
    public void DisplayUsingSeriesMessage()
    {
        consoleService.WriteLine($"  Using series numbers defined in {nameof(_appSettings.AlgorithmSettings.NumbersToUse)} (ignoring any in {nameof(_appSettings.AlgorithmSettings.NumbersToExclude)})");
    }

    /// <summary>
    /// Display a message indicating that the random numbers are being generated.
    /// </summary>
    public void DisplayGeneratingRandomNumbersMessage()
    {
        consoleService.WriteWithMarkup($"  Generating {_appSettings.AlgorithmSettings.RandomNumberTotal} random numbers from 1 to {_appSettings.AlgorithmSettings.RandomNumberMax}... ");
    }

    /// <summary>
    /// Display a message indicating that the process gave up generating random numbers.
    /// </summary>
    /// <param name="numberCount"></param>
    public void DisplayGaveUpGeneratingNumbersMessage(int numberCount)
    {
        consoleService.WriteLine($"  Gave up generating {_appSettings.AlgorithmSettings.RandomNumberTotal} random numbers. Generated {numberCount}");
    }

    /// <summary>
    /// Display a message indicating that the process is done.
    /// </summary>
    public void DisplayDone()
    {
        uiComponent.WriteDone();
    }
}