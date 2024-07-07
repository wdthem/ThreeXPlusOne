using Microsoft.Extensions.Options;
using System.Diagnostics;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App;

public class Process(IOptions<AppSettings> appSettings,
                     IAlgorithmService algorithmService,
                     IEnumerable<IDirectedGraph> directedGraphs,
                     IHistogramService histogramService,
                     IMetadataService metadataService,
                     IFileService fileService,
                     IConsoleService consoleService) : IScopedService
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private bool _generatedRandomNumbers = false;

    /// <summary>
    /// Run the algorithm and data generation based on the user-provided app settings.
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    public void Run(List<string> commandParsingMessages)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        consoleService.WriteAsciiArtLogo();
        consoleService.WriteCommandParsingMessages(commandParsingMessages);
        consoleService.WriteSettings();

        List<int> inputValues = GetInputValues(stopwatch);
        List<List<int>> seriesLists = algorithmService.Run(inputValues);

        metadataService.GenerateMedatadataFile(seriesLists);
        histogramService.GenerateHistogram(seriesLists);

        GenerateDirectedGraph(seriesLists);

        SaveSettings();

        stopwatch.Stop();

        consoleService.WriteProcessEnd(stopwatch.Elapsed);
    }

    /// <summary>
    /// Generate the directed graph based on app settings.
    /// </summary>
    /// <param name="seriesLists"></param>
    private void GenerateDirectedGraph(List<List<int>> seriesLists)
    {
        IDirectedGraph graph = directedGraphs.ToList()
                                             .Where(graph => graph.Dimensions == _appSettings.DirectedGraphAestheticSettings.SanitizedGraphDimensions)
                                             .First();

        consoleService.WriteHeading($"Directed graph ({graph.Dimensions}D)");

        graph.AddSeries(seriesLists);
        graph.PositionNodes();
        graph.SetCanvasDimensions();
        graph.SetNodeAesthetics();

        //allow the user to bail on generating the graph (for example, if canvas dimensions are too large)
        bool confirmedGenerateGraph = consoleService.ReadYKeyToProceed($"Generate {graph.Dimensions}D visualization?");

        if (!confirmedGenerateGraph)
        {
            consoleService.WriteLine("\nGraph generation cancelled\n");

            return;
        }

        consoleService.WriteLine("");

        graph.Draw();
    }

    /// <summary>
    /// Allow the user to save the generated number list to app settings for future use.
    /// </summary>
    private void SaveSettings()
    {
        consoleService.WriteHeading("Save app settings");

        bool confirmedSaveSettings = _generatedRandomNumbers &&
                                     consoleService.ReadYKeyToProceed($"Save generated number series to '{_appSettings.SettingsFileFullPath}' for reuse?");

        fileService.WriteSettingsToFile(confirmedSaveSettings);
        consoleService.WriteSettingsSavedMessage(confirmedSaveSettings);
    }

    /// <summary>
    /// Get or generate the list of numbers to use to run through the algorithm.
    /// Either:
    ///     The list specified by the user in app settings (this takes priority); or
    ///     Random numbers - the total number specified in app settings.
    /// </summary>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private List<int> GetInputValues(Stopwatch stopwatch)
    {
        consoleService.WriteHeading("Series data");

        List<int> inputValues = [];

        if (_appSettings.AlgorithmSettings.ListOfRandomNumbers.Count > 0)
        {
            inputValues = _appSettings.AlgorithmSettings.ListOfRandomNumbers;
            inputValues.RemoveAll(_appSettings.AlgorithmSettings.ListOfNumbersToExclude.Contains);

            if (inputValues.Count == 0)
            {
                throw new ApplicationException($"{nameof(_appSettings.AlgorithmSettings.NumbersToUse)} had values, but {nameof(_appSettings.AlgorithmSettings.NumbersToExclude)} removed them all. Please provide more numbers in {nameof(_appSettings.AlgorithmSettings.NumbersToUse)}");
            }

            _generatedRandomNumbers = false;

            consoleService.WriteLine($"Using series numbers defined in {nameof(_appSettings.AlgorithmSettings.NumbersToUse)} apart from any excluded in {nameof(_appSettings.AlgorithmSettings.NumbersToExclude)}\n");

            return inputValues;
        }

        consoleService.Write($"Generating {_appSettings.AlgorithmSettings.RandomNumberTotal} random numbers from 1 to {_appSettings.AlgorithmSettings.RandomNumberMax}... ");

        while (inputValues.Count < _appSettings.AlgorithmSettings.RandomNumberTotal)
        {
            if (stopwatch.Elapsed.TotalSeconds >= 10)
            {
                if (inputValues.Count == 0)
                {
                    throw new ApplicationException($"No numbers generated on which to run the algorithm. Check {nameof(_appSettings.AlgorithmSettings.NumbersToExclude)}");
                }

                consoleService.WriteLine($"\nGave up generating {_appSettings.AlgorithmSettings.RandomNumberTotal} random numbers. Generated {inputValues.Count}\n");

                break;
            }

            int randomValue = Random.Shared.Next(_appSettings.AlgorithmSettings.RandomNumberMax) + 1;

            if (_appSettings.AlgorithmSettings.ListOfNumbersToExclude.Contains(randomValue))
            {
                continue;
            }

            if (!inputValues.Contains(randomValue))
            {
                inputValues.Add(randomValue);
            }
        }

        //populate the property as the number list is used to generate a hash value for the directory name
        _appSettings.AlgorithmSettings.NumbersToUse = string.Join(", ", inputValues);
        _generatedRandomNumbers = true;

        consoleService.WriteDone();

        return inputValues;
    }
}