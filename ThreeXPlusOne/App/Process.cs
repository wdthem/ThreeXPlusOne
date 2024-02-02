﻿using Microsoft.Extensions.Options;
using System.Diagnostics;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces;
using ThreeXPlusOne.App.Interfaces.DirectedGraph;
using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App;

public class Process(IOptions<Settings> settings,
                     IAlgorithm algorithm,
                     IEnumerable<IDirectedGraph> directedGraphs,
                     IHistogram histogram,
                     IMetadata metadata,
                     IFileService fileService,
                     IConsoleService consoleService) : IProcess
{
    private readonly Settings _settings = settings.Value;
    private bool _generatedRandomNumbers = false;

    /// <summary>
    /// Run the algorithm and data generation based on the user-provided settings
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
    /// Generate the correct directed graph based on settings
    /// </summary>
    /// <param name="seriesLists"></param>
    private void GenerateDirectedGraph(List<List<int>> seriesLists)
    {
        IDirectedGraph graph = directedGraphs.ToList()
                                             .Where(graph => graph.Dimensions == _settings.SanitizedGraphDimensions)
                                             .First();

        consoleService.WriteHeading($"Directed graph ({graph.Dimensions}D)");

        if (!_settings.GenerateGraph)
        {
            consoleService.WriteLine("Graph generation disabled\n");

            return;
        }

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
    /// Allow the user to save the generated number list to settings for future use
    /// </summary>
    private void SaveSettings()
    {
        consoleService.WriteHeading("Save settings");

        bool confirmedSaveSettings = _generatedRandomNumbers &&
                                     consoleService.ReadYKeyToProceed($"Save generated number series to '{_settings.SettingsFileFullPath}' for reuse?");

        fileService.WriteSettingsToFile(confirmedSaveSettings);
        consoleService.WriteSettingsSavedMessage(confirmedSaveSettings);
    }

    /// <summary>
    /// Get or generate the list of numbers to use to run through the algorithm
    /// Either:
    ///     The list specified by the user in settings (this takes priority); or
    ///     Random numbers - the total number specified in settings
    /// </summary>
    /// <param name="stopwatch"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private List<int> GetInputValues(Stopwatch stopwatch)
    {
        consoleService.WriteHeading("Series data");

        List<int> inputValues = [];

        if (_settings.ListOfSeriesNumbers.Count > 0)
        {
            inputValues = _settings.ListOfSeriesNumbers;
            inputValues.RemoveAll(_settings.ListOfNumbersToExclude.Contains);

            if (inputValues.Count == 0)
            {
                throw new Exception($"{nameof(_settings.UseTheseNumbers)} had values, but {nameof(_settings.ExcludeTheseNumbers)} removed them all. Please provide more numbers in {nameof(_settings.UseTheseNumbers)}");
            }

            _generatedRandomNumbers = false;

            consoleService.WriteLine($"Using series numbers defined in {nameof(_settings.UseTheseNumbers)} apart from any excluded in {nameof(_settings.ExcludeTheseNumbers)}\n");

            return inputValues;
        }

        consoleService.Write($"Generating {_settings.NumberOfSeries} random numbers from 1 to {_settings.MaxStartingNumber}... ");

        while (inputValues.Count < _settings.NumberOfSeries)
        {
            if (stopwatch.Elapsed.TotalSeconds >= 10)
            {
                if (inputValues.Count == 0)
                {
                    throw new Exception($"No numbers generated on which to run the algorithm. Check {nameof(_settings.ExcludeTheseNumbers)}");
                }

                consoleService.WriteLine($"\nGave up generating {_settings.NumberOfSeries} random numbers. Generated {inputValues.Count}\n");

                break;
            }

            int randomValue = Random.Shared.Next(0, _settings.MaxStartingNumber) + 1;

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

        consoleService.WriteDone();

        return inputValues;
    }
}