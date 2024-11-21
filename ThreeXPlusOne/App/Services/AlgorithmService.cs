using System.Diagnostics;
using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services;

public class AlgorithmService(IOptions<AppSettings> appSettings,
                              IMetadataService metadataService,
                              IConsoleService consoleService) : IAlgorithmService
{
    private readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Run the algorithm and generate the related metadata and histogram.
    /// </summary>
    /// <remarks>
    /// The metadata is related to the algorithm's output, thus it is generated here.
    /// </remarks>
    /// <returns></returns>
    public async Task<List<CollatzResult>> Run()
    {
        List<CollatzResult> collatzResults = RunAlgorithm();

        await metadataService.GenerateMetadata(collatzResults);

        return collatzResults;
    }

    /// <summary>
    /// Execute the 3x+1 algorithm for all numbers either supplied by the user or generated randomly.
    /// </summary>
    /// <returns></returns>
    private List<CollatzResult> RunAlgorithm()
    {
        consoleService.WriteHeading("Algorithm execution");

        List<int> inputValues = GetInputValues();

        if (inputValues.Count == 0)
        {
            throw new ApplicationException("No input provided to the algorithm");
        }

        consoleService.WriteWithColorMarkup($"Running 3x + 1 algorithm on {inputValues.Count} numbers... ");

        List<CollatzResult> collatzResults = [];

        foreach (int value in inputValues)
        {
            if (value <= 0)
            {
                continue;
            }

            CollatzResult collatzResult = new()
            {
                StartingValue = value,
                Values = _appSettings.AlgorithmSettings.UseShortcutAlgorithm
                                        ? RunShortcutAlgorithm(value)
                                        : RunStandardAlgorithm(value)
            };

            collatzResults.Add(collatzResult);
        }

        consoleService.WriteDone();

        return collatzResults;
    }

    /// <summary>
    /// The standard Collatz Conjecture algorithm.
    /// </summary>
    /// <param name="value">The starting value on which to run the algorithm</param>
    private static List<int> RunStandardAlgorithm(int value)
    {
        //start the list with the initial value
        List<int> seriesList = [value];

        //avoid the infinite loop of 4, 2, 1 by stopping when the algorithm hits 1
        while (value > 1)
        {
            if (value % 2 == 0)
            {
                value /= 2;
            }
            else
            {
                value = (value * 3) + 1;
            }

            seriesList.Add(value);
        }

        return seriesList;
    }

    /// <summary>
    /// The shortcut Collatz Conjecture algorithm, which uses bitwise shifting for even numbers to 
    /// increase the speed of division calculations.
    /// </summary>
    /// <remarks>
    /// The result is fewer calculated values as sequences of consecutive even numbers will be skipped.
    /// </remarks>
    /// <param name="value">The starting value on which to run the algorithm</param>
    private static List<int> RunShortcutAlgorithm(int value)
    {
        //start the list with the initial value
        List<int> seriesList = [value];

        //avoid the infinite loop of 4, 2, 1 by stopping when the algorithm hits 1
        while (value > 1)
        {
            // Check if the number is even using bitwise AND
            if ((value & 1) == 0)
            {
                // Divide by 2 until the result is odd
                while ((value & 1) == 0 && value > 1)
                {
                    value >>= 1; // Divide by 2 using bitwise right-shift
                }
            }
            else
            {
                value = (value * 3) + 1;
            }

            seriesList.Add(value);
        }

        return seriesList;
    }

    /// <summary>
    /// Get or generate the list of numbers to use to run through the algorithm.
    /// Either:
    ///     The list specified by the user in app settings (this takes priority); or
    ///     Random numbers - the total number specified in app settings.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    private List<int> GetInputValues()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        List<int> inputValues = [];

        if (_appSettings.AlgorithmSettings.ListOfNumbersToUse.Count > 0)
        {
            inputValues = _appSettings.AlgorithmSettings.ListOfNumbersToUse;
            inputValues.RemoveAll(_appSettings.AlgorithmSettings.ListOfNumbersToExclude.Contains);

            if (inputValues.Count == 0)
            {
                throw new ApplicationException($"{nameof(_appSettings.AlgorithmSettings.NumbersToUse)} had values, but {nameof(_appSettings.AlgorithmSettings.NumbersToExclude)} removed them all. Please provide more numbers in {nameof(_appSettings.AlgorithmSettings.NumbersToUse)}");
            }

            _appSettings.AlgorithmSettings.FromRandomNumbers = false;

            consoleService.WriteLine($"Using series numbers defined in {nameof(_appSettings.AlgorithmSettings.NumbersToUse)} (ignoring any in {nameof(_appSettings.AlgorithmSettings.NumbersToExclude)})");

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

                consoleService.WriteLine($"Gave up generating {_appSettings.AlgorithmSettings.RandomNumberTotal} random numbers. Generated {inputValues.Count}");

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
        _appSettings.AlgorithmSettings.FromRandomNumbers = true;

        consoleService.WriteDone();

        return inputValues;
    }
}