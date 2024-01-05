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

        GenerateDirectedGraph(seriesLists);

        SaveSettings();

        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;

        string elapsedTime = string.Format("{0:00}:{1:00}.{2:000}",
                                           ts.Minutes, ts.Seconds, ts.Milliseconds);

        consoleHelper.WriteHeading($"Process completed");
        consoleHelper.WriteLine($"Execution time: {elapsedTime}\n\n");
    }

    /// <summary>
    /// Generate the correct directed graph based on settings
    /// </summary>
    /// <param name="seriesLists"></param>
    private void GenerateDirectedGraph(List<List<int>> seriesLists)
    {
        IDirectedGraph graph = directedGraphs.ToList()
                                             .Where(graph => graph.Dimensions == _settings.SanitizedGraphDimensions)
                                             .First();

        consoleHelper.WriteHeading($"Directed graph ({graph.Dimensions}D)");

        if (!_settings.GenerateGraph)
        {
            consoleHelper.WriteLine("Graph generation disabled\n");

            return;
        }

        graph.AddSeries(seriesLists);
        graph.PositionNodes();
        graph.SetCanvasDimensions();

        //allow the user to bail on generating the graph (for example, if canvas dimensions are too large)
        bool confirmedGenerateGraph = consoleHelper.ReadYKeyToProceed($"Generate {_settings.SanitizedGraphDimensions}D visualization?");

        if (!confirmedGenerateGraph)
        {
            consoleHelper.WriteLine("\nGraph generation cancelled\n");

            return;
        }

        graph.Draw();
    }

    /// <summary>
    /// Allow the user to save the generated number list to settings for future use
    /// </summary>
    private void SaveSettings()
    {
        consoleHelper.WriteHeading("Save settings");

        bool confirmedSaveSettings = _generatedRandomNumbers &&
                                     consoleHelper.ReadYKeyToProceed($"Save generated number series to '{_settings.SettingsFileName}' for reuse?");

        fileHelper.WriteSettingsToFile(confirmedSaveSettings);
        consoleHelper.WriteSettingsSavedMessage(confirmedSaveSettings);
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

        Random random = new();
        List<int> inputValues = [];

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