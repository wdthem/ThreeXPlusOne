﻿using Microsoft.Extensions.Options;
using System.Diagnostics;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App;

public class Process(IOptions<AppSettings> appSettings,
                     IAlgorithm algorithm,
                     IEnumerable<IDirectedGraph> directedGraphs,
                     IHistogram histogram,
                     IMetadata metadata,
                     IFileService fileService,
                     IConsoleService consoleService) : IProcess
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private bool _generatedRandomNumbers = false;

    /// <summary>
    /// Run the algorithm and data generation based on the user-provided app settings
    /// </summary>
    /// <param name="commandParsingMessages"></param>
    public void Run(List<string> commandParsingMessages)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        consoleService.WriteAsciiArtLogo();
        consoleService.WriteCommandParsingMessages(commandParsingMessages);
        consoleService.WriteSettings();

        List<int> inputValues = GetInputValues(stopwatch);
        List<List<int>> seriesLists = algorithm.Run(inputValues);

        metadata.GenerateMedatadataFile(seriesLists);
        histogram.GenerateHistogram(seriesLists);

        GenerateDirectedGraph(seriesLists);

        SaveSettings();

        stopwatch.Stop();

        consoleService.WriteProcessEnd(stopwatch.Elapsed);
    }

    /// <summary>
    /// Generate the correct directed graph based on app settings
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
        graph.SetNodeShapes();
        graph.SetCanvasDimensions();

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
    /// Allow the user to save the generated number list to app settings for future use
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
    /// Get or generate the list of numbers to use to run through the algorithm
    /// Either:
    ///     The list specified by the user in app settings (this takes priority); or
    ///     Random numbers - the total number specified in app settings
    /// </summary>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private List<int> GetInputValues(Stopwatch stopwatch)
    {
        consoleService.WriteHeading("Series data");

        List<int> inputValues = [];

        if (_appSettings.AlgorithmSettings.ListOfSeriesNumbers.Count > 0)
        {
            inputValues = _appSettings.AlgorithmSettings.ListOfSeriesNumbers;
            inputValues.RemoveAll(_appSettings.AlgorithmSettings.ListOfNumbersToExclude.Contains);

            if (inputValues.Count == 0)
            {
                throw new Exception($"{nameof(_appSettings.AlgorithmSettings.UseTheseNumbers)} had values, but {nameof(_appSettings.AlgorithmSettings.ExcludeTheseNumbers)} removed them all. Please provide more numbers in {nameof(_appSettings.AlgorithmSettings.UseTheseNumbers)}");
            }

            _generatedRandomNumbers = false;

            consoleService.WriteLine($"Using series numbers defined in {nameof(_appSettings.AlgorithmSettings.UseTheseNumbers)} apart from any excluded in {nameof(_appSettings.AlgorithmSettings.ExcludeTheseNumbers)}\n");

            return inputValues;
        }

        consoleService.Write($"Generating {_appSettings.AlgorithmSettings.NumberOfSeries} random numbers from 1 to {_appSettings.AlgorithmSettings.MaxStartingNumber}... ");

        while (inputValues.Count < _appSettings.AlgorithmSettings.NumberOfSeries)
        {
            if (stopwatch.Elapsed.TotalSeconds >= 10)
            {
                if (inputValues.Count == 0)
                {
                    throw new Exception($"No numbers generated on which to run the algorithm. Check {nameof(_appSettings.AlgorithmSettings.ExcludeTheseNumbers)}");
                }

                consoleService.WriteLine($"\nGave up generating {_appSettings.AlgorithmSettings.NumberOfSeries} random numbers. Generated {inputValues.Count}\n");

                break;
            }

            int randomValue = Random.Shared.Next(0, _appSettings.AlgorithmSettings.MaxStartingNumber) + 1;

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
        _appSettings.AlgorithmSettings.UseTheseNumbers = string.Join(", ", inputValues);
        _generatedRandomNumbers = true;

        consoleService.WriteDone();

        return inputValues;
    }
}