using ThreeXPlusOne.App.Interfaces.Services;

namespace ThreeXPlusOne.App.Services;

public class AlgorithmService(IConsoleService consoleService) : IAlgorithmService
{
    /// <summary>
    /// Execute the 3x+1 algorithm for all numbers either supplied by the user or generated randomly
    /// </summary>
    /// <param name="inputValues"></param>
    /// <returns></returns>
    public List<List<int>> Run(List<int> inputValues)
    {
        consoleService.WriteHeading("Algorithm execution");

        if (inputValues.Count == 0)
        {
            throw new ApplicationException("No input provided to the algorithm");
        }

        consoleService.Write($"Running 3x + 1 algorithm on {inputValues.Count} numbers... ");

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

        consoleService.WriteDone();

        return returnValues;
    }
}
