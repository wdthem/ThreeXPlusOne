using ThreeXPlusOne.Code.Interfaces;
using ThreeXPlusOne.Code.Interfaces.Helpers;

namespace ThreeXPlusOne.Code;

public class Algorithm(IConsoleHelper consoleHelper) : IAlgorithm
{
    /// <summary>
    /// Execute the the 3x+1 algorithm for all numbers either supplied by the user or generated randomly
    /// </summary>
    /// <param name="inputValues"></param>
    /// <returns></returns>
    public List<List<int>> Run(List<int> inputValues)
    {
        consoleHelper.WriteHeading("Algorithm execution");

        if (inputValues.Count == 0)
        {
            throw new Exception("No input provided to the algorithm");
        }

        consoleHelper.Write($"Running 3x + 1 algorithm on {inputValues.Count} numbers... ");

        List<List<int>> returnValues = [];

        foreach (int value in inputValues)
        {
            if (value <= 0)
            {
                continue;
            }

            List<int> outputValues = [];

            int calculatedValue = value;

            //add the first number in the series
            outputValues.Add(calculatedValue);

            //avoid the infinite loop of 4, 2, 1 by stopping when the algorithm hits 1
            while (calculatedValue > 1)
            {
                //perform the two rules of the Collatz Conjecture
                if (calculatedValue % 2 == 0)
                {
                    calculatedValue /= 2;
                }
                else
                {
                    calculatedValue = (calculatedValue * 3) + 1;
                }

                outputValues.Add(calculatedValue);
            }

            returnValues.Add(outputValues);
        }

        consoleHelper.WriteDone();

        return returnValues;
    }
}