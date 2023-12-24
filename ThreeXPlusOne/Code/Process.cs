using System.Diagnostics;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Config;

namespace ThreeXPlusOne.Code;

public class Process(IOptions<Settings> settings,
                     IAlgorithm algorithm,
                     IEnumerable<IDirectedGraph> directedGraphs,
                     IHistogram histogram,
                     IMetadata metadata,
                     IFileHelper fileHelper,
                     IConsoleHelper consoleHelper) : IProcess
{
    private readonly Settings _settings = settings.Value;
    private bool _generatedRandomNumbers = false;

    /// <summary>
    /// Run the algorithm and data generation based on the user-provided settings
    /// </summary>
    public void Run()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();

        consoleHelper.WriteAsciiArtLogo();
        consoleHelper.WriteSettings();

        List<int> inputValues = GenerateInputValues(stopwatch);
        List<List<int>> seriesLists = algorithm.Run(inputValues);

        metadata.GenerateMedatadataFile(seriesLists);
        histogram.GenerateHistogram(seriesLists);

        IDirectedGraph graph = directedGraphs.ToList()
                                             .Where(graph => graph.Dimensions == _settings.SanitizedGraphDimensions)
                                             .First();

        consoleHelper.WriteHeading($"Directed graph ({graph.Dimensions}D)");

        graph.AddSeries(seriesLists);
        graph.PositionNodes();
        graph.Draw();

        consoleHelper.WriteHeading("Save settings");

        bool confirmedSaveSettings = _generatedRandomNumbers &&
                                     consoleHelper.ReadYKeyToProceed($"Save generated number series to '{_settings.SettingsFileName}' for reuse?");

        fileHelper.WriteSettingsToFile(confirmedSaveSettings);
        consoleHelper.WriteSettingsSavedMessage(confirmedSaveSettings);

        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;

        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           ts.Minutes, ts.Seconds, ts.Milliseconds);

        consoleHelper.WriteHeading($"Process completed");
        consoleHelper.WriteLine($"Execution time: {elapsedTime}\n\n");
    }

    /// <summary>
    /// Get the list of numbers to use to run through the algorithm
    /// Either:
    ///     Random numbers - the amount specified in settings; or
    ///     The list specified by the user in settings (this takes priority)
    /// </summary>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    private List<int> GenerateInputValues(Stopwatch stopwatch)
    {
        consoleHelper.WriteHeading("Series data");

        var random = new Random();
        var inputValues = new List<int>();

        if (string.IsNullOrWhiteSpace(_settings.UseTheseNumbers))
        {
            consoleHelper.Write($"Generating {_settings.NumberOfSeries} random numbers from 1 to {_settings.MaxStartingNumber}... ");

            while (inputValues.Count < _settings.NumberOfSeries)
            {
                if (stopwatch.Elapsed.TotalSeconds >= 10)
                {
                    if (inputValues.Count == 0)
                    {
                        throw new Exception($"No numbers generated on which to run the algorithm. Check {nameof(_settings.ExcludeTheseNumbers)}");
                    }

                    consoleHelper.WriteLine($"\nGave up generating {_settings.NumberOfSeries} random numbers. Generated {inputValues.Count}\n");

                    break;
                }

                int randomValue = random.Next(0, _settings.MaxStartingNumber) + 1;

                if (_settings.ListOfNumbersToExclude.Contains(randomValue))
                {
                    continue;
                }

                if (!inputValues.Contains(randomValue))
                {
                    inputValues.Add(randomValue);
                }
            }

            //populate the property as the number list is used to generate a hash value for the directory name
            _settings.UseTheseNumbers = string.Join(", ", inputValues);
            _generatedRandomNumbers = true;

            consoleHelper.WriteDone();
        }
        else
        {
            inputValues = _settings.ListOfSeriesNumbers;
            inputValues.RemoveAll(_settings.ListOfNumbersToExclude.Contains);

            if (inputValues.Count == 0)
            {
                throw new Exception("No numbers provided on which to run the algorithm");
            }

            _generatedRandomNumbers = false;

            consoleHelper.WriteLine($"Using series numbers defined in {nameof(_settings.UseTheseNumbers)} apart from any excluded in {nameof(_settings.ExcludeTheseNumbers)}\n");
        }

        return inputValues;
    }
}