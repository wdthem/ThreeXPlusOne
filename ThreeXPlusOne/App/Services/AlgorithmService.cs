using Microsoft.Extensions.Options;
using ThreeXPlusOne.App.Config;
using ThreeXPlusOne.App.Interfaces.Services;
using ThreeXPlusOne.App.Models;

namespace ThreeXPlusOne.App.Services;

public class AlgorithmService(IOptions<AppSettings> appSettings,
                              IConsoleService consoleService) : IAlgorithmService
{
    protected readonly AppSettings _appSettings = appSettings.Value;

    /// <summary>
    /// Execute the 3x+1 algorithm for all numbers either supplied by the user or generated randomly.
    /// </summary>
    /// <param name="inputValues"></param>
    /// <returns></returns>
    public List<CollatzResult> Run(List<int> inputValues)
    {
        consoleService.WriteHeading("Algorithm execution");

        if (inputValues.Count == 0)
        {
            throw new ApplicationException("No input provided to the algorithm");
        }

        consoleService.Write($"Running 3x + 1 algorithm on {inputValues.Count} numbers... ");

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
                Values = [value] //add the first number in the series
            };

            if (_appSettings.AlgorithmSettings.UseShortcutAlgorithm)
            {
                ShortcutCollatzAlgorithm(value, collatzResult.Values);
            }
            else
            {
                StandardCollatzAlgorithm(value, collatzResult.Values);
            }

            collatzResults.Add(collatzResult);
        }

        consoleService.WriteDone();

        return collatzResults;
    }

    /// <summary>
    /// The standard Collatz Conjecture algorithm.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="calculatedValues"></param>
    private static void StandardCollatzAlgorithm(int value,
                                                 List<int> calculatedValues)
    {
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

            calculatedValues.Add(value);
        }
    }

    /// <summary>
    /// The shortcut Collatz Conjecture algorithm, which uses bitwise shifting for even numbers to 
    /// increase the speed of division calculations.
    /// </summary>
    /// <remarks>
    /// The result is fewer calculated values as sequences of even numbers will be skipped.
    /// </remarks>
    /// <param name="value"></param>
    /// <param name="calculatedValues"></param>
    private static void ShortcutCollatzAlgorithm(int value,
                                                 List<int> calculatedValues)
    {
        //avoid the infinite loop of 4, 2, 1 by stopping when the algorithm hits 1
        while (value > 1)
        {
            if ((value & 1) == 0) // Check if the number is even using bitwise AND
            {
                // Divide by 2 until the result is odd
                while ((value & 1) == 0 && value > 1)
                {
                    value >>= 1; // Divide by 2 using bitwise shift
                }
            }
            else
            {
                value = (value * 3) + 1;
            }

            calculatedValues.Add(value);
        }
    }
}